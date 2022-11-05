using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

public class AgentManager : MonoBehaviour {

    [Header("Agents")]
    public int NumberToSpawn = 100;
    public Vector3 SpawnMinPosition = new Vector3(-20, 1, -10);
    public Vector3 SpawnMaxPosition = new Vector3(20, 1, 10);
    public GameObject AgentToSpawn = null;
    public string AgentTagToLookFor = null;

    [Header("Neighborhood")]
    public CalcEveryNFrameOption CalcEveryNFrames = CalcEveryNFrameOption._1;
    public NeighborhoodCalcMode NeighborhoodCalcMode = NeighborhoodCalcMode.CPUOctree;

    [Header("Neighborhood CPU")]
    public CPUTreeDepthOption CPUTreeDepth = CPUTreeDepthOption._3;
    public CPUTreeBucketSizeOption CPUTreeBucketSize = CPUTreeBucketSizeOption._8;
    public bool DirectToNode = false;

    [Header("Neighborhood GPU")]
    public GPUTreeDepthOption GPUTreeDepth = GPUTreeDepthOption._3;
    public ComputeShader NeighborhoodShader;
    public int GPUNeighborhoodCount = 16;

    [Header("Fuzzy")]
    public FuzzyController FuzzyController;
    public FuzzySetOperationsMode FuzzySetOperationsMode = FuzzySetOperationsMode.MinimumAndMaximum;

    private List<GameObject> agents;
    private List<Agent> agentsAgentScripts;
    private int agentsCount;
    private bool hasAgentsCount = false;
    private Octree octree;

    private int threadsInGroup = 64;
    private Dictionary<int, int> kernel2n;
    private Dictionary<int, int> octreeLeafsKernel;
    private Dictionary<int, int> octreeNeighborsKernel;

    private Texture3D octreeTexture;

    private RenderTexture treeLeafsTexture;
    private RenderTexture treeNeighborsTexture;

    private int treeInnerNodesCount;
    private int treeInnerInnerNodesCount;
    private int treeLeafsCount;

    private ComputeBuffer positionBuffer;
    private ComputeBuffer directionBuffer;
    private ComputeBuffer perceptionRadiusBuffer;
    private ComputeBuffer horizontalFOVBuffer;
    private ComputeBuffer verticalFOVBuffer;

    private ComputeBuffer neighborhoodBuffer;
    private int[] neighborhood;

    private int updateCounter = 1;

    void Start() {
        this.agents = new List<GameObject>();

        List<GameObject> foundAgents = FindAgents();
        this.agents.AddRange(foundAgents);

        List<GameObject> spawnedAgents = SpawnAgents();
        this.agents.AddRange(spawnedAgents);

        System.Random rnd = new System.Random();
        this.agents = this.agents.OrderBy(item => rnd.Next()).ToList();

        this.agentsAgentScripts = agents.Select(agent => agent.GetComponent<Agent>()).ToList();

        this.agentsCount = this.agentsAgentScripts.Count;
        this.hasAgentsCount = true;

        while (this.threadsInGroup > this.agentsCount) {
            this.threadsInGroup /= 2;
        }
        if (this.threadsInGroup <= 0) this.threadsInGroup = 1;

        this.FuzzyController.SetFuzzySetOperationType(this.FuzzySetOperationsMode);

        foreach (Agent agent in this.agentsAgentScripts) {
            agent.SetFuzzyController(this.FuzzyController);
        }

        switch (this.NeighborhoodCalcMode) {
            case NeighborhoodCalcMode.GPUNaiveN2:
                this.SetupShaderGPU2n();
                break;
            case NeighborhoodCalcMode.GPUOctree:
                this.SetupShaderGPUOctree();
                this.BuildGPUOctree();
                break;
        }

        this.CreateBuffers();
    }

    void OnEnable() {
        if (this.hasAgentsCount) {
            this.CreateBuffers();
        }
    }

    void OnDisable() {
        this.DisposeBuffers();
    }

    private void CreateBuffers() {
        if (this.NeighborhoodCalcMode == NeighborhoodCalcMode.GPUOctree ||
            this.NeighborhoodCalcMode == NeighborhoodCalcMode.GPUNaiveN2) {
            
            this.positionBuffer = new ComputeBuffer(this.agentsCount, 3 * 4);
            this.directionBuffer = new ComputeBuffer(this.agentsCount, 3 * 4);
            this.perceptionRadiusBuffer = new ComputeBuffer(this.agentsCount, 4);
            this.horizontalFOVBuffer = new ComputeBuffer(this.agentsCount, 4);
            this.verticalFOVBuffer = new ComputeBuffer(this.agentsCount, 4);
            this.neighborhoodBuffer = new ComputeBuffer(this.agentsCount * this.GPUNeighborhoodCount, 4);
        }
    }

    private void DisposeBuffers() {
        if (this.NeighborhoodCalcMode == NeighborhoodCalcMode.GPUOctree ||
            this.NeighborhoodCalcMode == NeighborhoodCalcMode.GPUNaiveN2) {
            
            this.positionBuffer.Dispose();
            this.directionBuffer.Dispose();
            this.perceptionRadiusBuffer.Dispose();
            this.horizontalFOVBuffer.Dispose();
            this.verticalFOVBuffer.Dispose();
            this.neighborhoodBuffer.Dispose();
        }
    }

    void Update() {

        if (this.updateCounter < (int)this.CalcEveryNFrames) {
            this.updateCounter++;
            return;
        }
        this.updateCounter = 1;

        switch (this.NeighborhoodCalcMode) {
            case NeighborhoodCalcMode.CPUNaiveN2:
                this.SetNeighborsCPU2n();
                break;
            case NeighborhoodCalcMode.CPUOctree:
                this.BuildCPUOctree();
                this.SetNeighborsCPUOctree();
                break;
            case NeighborhoodCalcMode.GPUNaiveN2:
                this.DispatchGPU2n();
                break;
            case NeighborhoodCalcMode.GPUOctree:
                this.BuildGPUOctreeLeafs();
                break;
        }
    }

    private List<GameObject> FindAgents() {
        List<GameObject> foundAgents = new List<GameObject>();
        if (!string.IsNullOrEmpty(this.AgentTagToLookFor)) {
            foundAgents.AddRange(GameObject.FindGameObjectsWithTag(this.AgentTagToLookFor));
        }
        return foundAgents;
    }

    private List<GameObject> SpawnAgents() {
        List<GameObject> spawnedAgents = new List<GameObject>();
        for (int i = 0; i < this.NumberToSpawn; i++) {
            GameObject newAgent = Instantiate(this.AgentToSpawn, new Vector3(Random.Range(this.SpawnMinPosition.x, this.SpawnMaxPosition.x), Random.Range(this.SpawnMinPosition.y, this.SpawnMaxPosition.y), Random.Range(this.SpawnMinPosition.z, this.SpawnMaxPosition.z)), Quaternion.identity);
            newAgent.transform.parent = this.gameObject.transform;
            spawnedAgents.Add(newAgent);
        }
        return spawnedAgents;
    }

    private void BuildCPUOctree() {
        float xmin = float.MaxValue;
        float xmax = float.MinValue;
        float ymin = float.MaxValue;
        float ymax = float.MinValue;
        float zmin = float.MaxValue;
        float zmax = float.MinValue;

        foreach (Agent agent in this.agentsAgentScripts) {
            xmin = Mathf.Min(xmin, agent.Position.x);
            xmax = Mathf.Max(xmax, agent.Position.x);
            ymin = Mathf.Min(ymin, agent.Position.y);
            ymax = Mathf.Max(ymax, agent.Position.y);
            zmin = Mathf.Min(zmin, agent.Position.z);
            zmax = Mathf.Max(zmax, agent.Position.z);
        }

        this.octree = new Octree((int)this.CPUTreeDepth, (int)this.CPUTreeBucketSize, xmin, xmax, ymin, ymax, zmin, zmax);

        foreach (Agent agent in this.agentsAgentScripts) {
            this.octree.Insert(agent);
        }
    }

    private void SetNeighborsCPU2n() {
        foreach (Agent agent in this.agentsAgentScripts) {
            Vector3 f = agent.Direction.normalized;
            Vector3 l = Vector3.Cross(f, Vector3.up).normalized;
            Vector3 u = Vector3.Cross(l, f).normalized;
            List<Agent> neighbors = new List<Agent>();
            foreach (Agent otherAgent in this.agentsAgentScripts) {
                if (Utils.AgentInAgentFieldOfView(agent, otherAgent, f, l, u)) {
                    neighbors.Add(otherAgent);
                }
            }
            agent.Neighbors = neighbors;
        }
    }

    private void SetNeighborsCPUOctree() {
        if (this.octree != null) {
            foreach (Agent agent in this.agentsAgentScripts) {
                agent.Neighbors = this.octree.GetNeighbors(agent, this.DirectToNode);
            }
        }
    }

    private void SetupShaderGPU2n() {
        this.kernel2n = new Dictionary<int, int>();

        this.kernel2n[64] = this.NeighborhoodShader.FindKernel("Kernel2n64");
        this.kernel2n[32] = this.NeighborhoodShader.FindKernel("Kernel2n32");
        this.kernel2n[16] = this.NeighborhoodShader.FindKernel("Kernel2n16");
        this.kernel2n[8] = this.NeighborhoodShader.FindKernel("Kernel2n8");
        this.kernel2n[4] = this.NeighborhoodShader.FindKernel("Kernel2n4");
        this.kernel2n[2] = this.NeighborhoodShader.FindKernel("Kernel2n2");
        this.kernel2n[1] = this.NeighborhoodShader.FindKernel("Kernel2n1");

        this.treeNeighborsTexture = new RenderTexture(this.agentsCount, this.agentsCount, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
        {
            enableRandomWrite = true
        };
        this.treeNeighborsTexture.Create();

        this.neighborhood = new int[this.agentsCount * this.GPUNeighborhoodCount];
        for (int i = 0; i < this.neighborhood.Length; i++) {
            this.neighborhood[i] = -1;
        }

        this.NeighborhoodShader.SetInt("agentsCount", this.agentsCount);
        this.NeighborhoodShader.SetInt("neighborhoodCount", this.GPUNeighborhoodCount);
    }

    private void SetupShaderGPUOctree() {
        this.octreeLeafsKernel = new Dictionary<int, int>();

        this.octreeLeafsKernel[64] = this.NeighborhoodShader.FindKernel("OctreeLeafsKernel64");
        this.octreeLeafsKernel[32] = this.NeighborhoodShader.FindKernel("OctreeLeafsKernel32");
        this.octreeLeafsKernel[16] = this.NeighborhoodShader.FindKernel("OctreeLeafsKernel16");
        this.octreeLeafsKernel[8] = this.NeighborhoodShader.FindKernel("OctreeLeafsKernel8");
        this.octreeLeafsKernel[4] = this.NeighborhoodShader.FindKernel("OctreeLeafsKernel4");
        this.octreeLeafsKernel[2] = this.NeighborhoodShader.FindKernel("OctreeLeafsKernel2");
        this.octreeLeafsKernel[1] = this.NeighborhoodShader.FindKernel("OctreeLeafsKernel1");

        this.octreeNeighborsKernel = new Dictionary<int, int>();

        this.octreeNeighborsKernel[64] = this.NeighborhoodShader.FindKernel("OctreeNeighborsKernel64");
        this.octreeNeighborsKernel[32] = this.NeighborhoodShader.FindKernel("OctreeNeighborsKernel32");
        this.octreeNeighborsKernel[16] = this.NeighborhoodShader.FindKernel("OctreeNeighborsKernel16");
        this.octreeNeighborsKernel[8] = this.NeighborhoodShader.FindKernel("OctreeNeighborsKernel8");
        this.octreeNeighborsKernel[4] = this.NeighborhoodShader.FindKernel("OctreeNeighborsKernel4");
        this.octreeNeighborsKernel[2] = this.NeighborhoodShader.FindKernel("OctreeNeighborsKernel2");
        this.octreeNeighborsKernel[1] = this.NeighborhoodShader.FindKernel("OctreeNeighborsKernel1");

        int b = 8;
        int n = 0;
        for (int i = 0; i < (int)this.GPUTreeDepth - 1; i++) {
            n += (int)Mathf.Pow(b, i);
        }
        this.treeInnerInnerNodesCount = n;
        n += (int)Mathf.Pow(b, (int)this.GPUTreeDepth - 1);
        this.treeInnerNodesCount = n;
        this.treeLeafsCount = (int)Mathf.Pow(b, (int)this.GPUTreeDepth);

        // depth = 4; 85 cells; 170 * 2
        // depth = 3; 21 cells; 42 * 2
        this.octreeTexture = new Texture3D(this.treeInnerNodesCount * 2, 2, 2, TextureFormat.RGBAHalf, false);

        // depth = 4; 256 leaf cells
        // depth = 3; 64 leaf cells
        this.treeLeafsTexture = new RenderTexture(this.treeLeafsCount, this.agentsCount, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
        {
            enableRandomWrite = true
        };
        this.treeLeafsTexture.Create();

        this.treeNeighborsTexture = new RenderTexture(this.agentsCount, this.agentsCount, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
        {
            enableRandomWrite = true
        };
        this.treeNeighborsTexture.Create();

        this.neighborhood = new int[this.agentsCount * this.GPUNeighborhoodCount];
        for (int i = 0; i < this.neighborhood.Length; i++) {
            this.neighborhood[i] = -1;
        }

        this.NeighborhoodShader.SetInt("agentsCount", this.agentsCount);
        this.NeighborhoodShader.SetInt("neighborhoodCount", this.GPUNeighborhoodCount);
        this.NeighborhoodShader.SetInt("treeDepth", (int)this.GPUTreeDepth);
        this.NeighborhoodShader.SetInt("treeLeafsCount", this.treeLeafsCount);
    }

    private void BuildGPUOctree() {
        int c = 1;
        for (int d = 0; d < this.treeInnerInnerNodesCount; d++) {
            int x = d * 2;
            int y = 0;
            int z = 0;

            octreeTexture.SetPixel(x, y, z, new Color(c, 0, 0, 0));
            c++;

            octreeTexture.SetPixel(x, y, z + 1, new Color(c, 0, 0, 0));
            c++;

            octreeTexture.SetPixel(x, y + 1, z, new Color(c, 0, 0, 0));
            c++;

            octreeTexture.SetPixel(x, y + 1, z + 1, new Color(c, 0, 0, 0));
            c++;

            octreeTexture.SetPixel(x + 1, y, z, new Color(c, 0, 0, 0));
            c++;

            octreeTexture.SetPixel(x + 1, y, z + 1, new Color(c, 0, 0, 0));
            c++;

            octreeTexture.SetPixel(x + 1, y + 1, z, new Color(c, 0, 0, 0));
            c++;

            octreeTexture.SetPixel(x + 1, y + 1, z + 1, new Color(c, 0, 0, 0));
            c++;
        }


        int nx = 0;
        for (int d = this.treeInnerInnerNodesCount; d < this.treeInnerNodesCount; d++) {
            int x = d * 2;
            int y = 0;
            int z = 0;

            octreeTexture.SetPixel(x, y, z, new Color(nx, 0, 0, 0));
            nx++;

            octreeTexture.SetPixel(x, y, z + 1, new Color(nx, 0, 0, 0));
            nx++;

            octreeTexture.SetPixel(x, y + 1, z, new Color(nx, 0, 0, 0));
            nx++;

            octreeTexture.SetPixel(x, y + 1, z + 1, new Color(nx, 0, 0, 0));
            nx++;

            octreeTexture.SetPixel(x + 1, y, z, new Color(nx, 0, 0, 0));
            nx++;

            octreeTexture.SetPixel(x + 1, y, z + 1, new Color(nx, 0, 0, 0));
            nx++;

            octreeTexture.SetPixel(x + 1, y + 1, z, new Color(nx, 0, 0, 0));
            nx++;

            octreeTexture.SetPixel(x + 1, y + 1, z + 1, new Color(nx, 0, 0, 0));
            nx++;
        }

        this.octreeTexture.Apply();

        int leafsKernel = this.octreeLeafsKernel[this.threadsInGroup];
        int neighborsKernel = this.octreeNeighborsKernel[this.threadsInGroup];

        this.NeighborhoodShader.SetTexture(leafsKernel, "Octree", this.octreeTexture);
        this.NeighborhoodShader.SetTexture(neighborsKernel, "Octree", this.octreeTexture);
    }

    private void DispatchGPU2n() {
        Vector3[] positions = new Vector3[this.agentsCount];
        Vector3[] directions = new Vector3[this.agentsCount];
        float[] perceptionRadiuses = new float[this.agentsCount];
        float[] horizontalFOVs = new float[this.agentsCount];
        float[] verticalFOVs = new float[this.agentsCount];

        for (int i = 0; i < this.agentsAgentScripts.Count; i++) {
            Agent agent = this.agentsAgentScripts[i];

            positions[i] = agent.Position;
            directions[i] = agent.Direction;
            perceptionRadiuses[i] = agent.PerceptionRadius;
            horizontalFOVs[i] = agent.HorizontalFOV;
            verticalFOVs[i] = agent.VerticalFOV;
        }

        this.positionBuffer.SetData(positions);
        this.directionBuffer.SetData(directions);
        this.perceptionRadiusBuffer.SetData(perceptionRadiuses);
        this.horizontalFOVBuffer.SetData(horizontalFOVs);
        this.verticalFOVBuffer.SetData(verticalFOVs);

        int kernel = this.kernel2n[this.threadsInGroup];

        this.NeighborhoodShader.SetBuffer(kernel, "Position", this.positionBuffer);
        this.NeighborhoodShader.SetBuffer(kernel, "Direction", this.directionBuffer);
        this.NeighborhoodShader.SetBuffer(kernel, "PerceptionRadius", this.perceptionRadiusBuffer);
        this.NeighborhoodShader.SetBuffer(kernel, "HorizontalFOV", this.horizontalFOVBuffer);
        this.NeighborhoodShader.SetBuffer(kernel, "VerticalFOV", this.verticalFOVBuffer);
        this.NeighborhoodShader.SetTexture(kernel, "TreeNeighbors", this.treeNeighborsTexture);
        this.NeighborhoodShader.SetBuffer(kernel, "Neighborhood", this.neighborhoodBuffer);

        this.NeighborhoodShader.Dispatch(kernel, Mathf.CeilToInt(1.0f * this.agentsCount / this.threadsInGroup), 1, 1);

        AsyncGPUReadback.Request(this.neighborhoodBuffer, this.SetNeighborhood);
    }

    private void BuildGPUOctreeLeafs() {
        float xmin = float.MaxValue;
        float xmax = float.MinValue;
        float ymin = float.MaxValue;
        float ymax = float.MinValue;
        float zmin = float.MaxValue;
        float zmax = float.MinValue;

        Vector3[] positions = new Vector3[this.agentsCount];
        Vector3[] directions = new Vector3[this.agentsCount];
        float[] perceptionRadiuses = new float[this.agentsCount];
        float[] horizontalFOVs = new float[this.agentsCount];
        float[] verticalFOVs = new float[this.agentsCount];

        for (int i = 0; i < this.agentsAgentScripts.Count; i++) {
            Agent agent = this.agentsAgentScripts[i];

            xmin = Mathf.Min(xmin, agent.Position.x);
            xmax = Mathf.Max(xmax, agent.Position.x);
            ymin = Mathf.Min(ymin, agent.Position.y);
            ymax = Mathf.Max(ymax, agent.Position.y);
            zmin = Mathf.Min(zmin, agent.Position.z);
            zmax = Mathf.Max(zmax, agent.Position.z);

            positions[i] = agent.Position;
            directions[i] = agent.Direction;
            perceptionRadiuses[i] = agent.PerceptionRadius;
            horizontalFOVs[i] = agent.HorizontalFOV;
            verticalFOVs[i] = agent.VerticalFOV;
        }

        this.NeighborhoodShader.SetFloat("xmin", xmin);
        this.NeighborhoodShader.SetFloat("xmax", xmax);
        this.NeighborhoodShader.SetFloat("ymin", ymin);
        this.NeighborhoodShader.SetFloat("ymax", ymax);
        this.NeighborhoodShader.SetFloat("zmin", zmin);
        this.NeighborhoodShader.SetFloat("zmax", zmax);

        this.positionBuffer.SetData(positions);
        this.directionBuffer.SetData(directions);
        this.perceptionRadiusBuffer.SetData(perceptionRadiuses);
        this.horizontalFOVBuffer.SetData(horizontalFOVs);
        this.verticalFOVBuffer.SetData(verticalFOVs);

        int leafsKernel = this.octreeLeafsKernel[this.threadsInGroup];
        int neighborsKernel = this.octreeNeighborsKernel[this.threadsInGroup];

        this.NeighborhoodShader.SetBuffer(leafsKernel, "Position", this.positionBuffer);
        this.NeighborhoodShader.SetTexture(leafsKernel, "TreeLeafs", this.treeLeafsTexture);


        this.NeighborhoodShader.SetBuffer(neighborsKernel, "Position", this.positionBuffer);
        this.NeighborhoodShader.SetBuffer(neighborsKernel, "Direction", this.directionBuffer);
        this.NeighborhoodShader.SetBuffer(neighborsKernel, "PerceptionRadius", this.perceptionRadiusBuffer);
        this.NeighborhoodShader.SetBuffer(neighborsKernel, "HorizontalFOV", this.horizontalFOVBuffer);
        this.NeighborhoodShader.SetBuffer(neighborsKernel, "VerticalFOV", this.verticalFOVBuffer);
        this.NeighborhoodShader.SetTexture(neighborsKernel, "TreeLeafs", this.treeLeafsTexture);
        this.NeighborhoodShader.SetTexture(neighborsKernel, "TreeNeighbors", this.treeNeighborsTexture);
        this.NeighborhoodShader.SetBuffer(neighborsKernel, "Neighborhood", this.neighborhoodBuffer);


        this.NeighborhoodShader.Dispatch(leafsKernel, Mathf.CeilToInt(1.0f * this.agentsCount / this.threadsInGroup), 1, 1);
        this.NeighborhoodShader.Dispatch(neighborsKernel, Mathf.CeilToInt(1.0f * this.agentsCount / this.threadsInGroup), 1, 1);

        AsyncGPUReadback.Request(this.neighborhoodBuffer, this.SetNeighborhood);
    }

    private void SetNeighborhood(AsyncGPUReadbackRequest req) {
        if (req.done) {
            this.neighborhood = req.GetData<int>().ToArray();
            this.SetNeighborsGPU();
        }
    }

    private void SetNeighborsGPU() {
        for (int i = 0; i < this.agentsAgentScripts.Count; i++) {
            List<Agent> observed = new List<Agent>();

            for (int j = 0; j < this.GPUNeighborhoodCount; j++) {
                int n = this.neighborhood[i * this.GPUNeighborhoodCount + j];
                if (n >= 0) {
                    observed.Add(this.agentsAgentScripts[n]);
                }
            }

            this.agentsAgentScripts[i].Neighbors = observed;
        }
    }

}