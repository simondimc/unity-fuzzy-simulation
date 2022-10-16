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
    private Octree octree;

    private int kernel2n;
    private int octreeLeafsKernel;
    private int octreeNeighborsKernel;

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
    }

    void OnEnable() {
        if (this.NeighborhoodCalcMode == NeighborhoodCalcMode.GPUOctree ||
            this.NeighborhoodCalcMode == NeighborhoodCalcMode.GPUNaiveN2) {
            
            this.positionBuffer = new ComputeBuffer(this.NumberToSpawn, 3 * 4);
            this.directionBuffer = new ComputeBuffer(this.NumberToSpawn, 3 * 4);
            this.perceptionRadiusBuffer = new ComputeBuffer(this.NumberToSpawn, 4);
            this.horizontalFOVBuffer = new ComputeBuffer(this.NumberToSpawn, 4);
            this.verticalFOVBuffer = new ComputeBuffer(this.NumberToSpawn, 4);
            this.neighborhoodBuffer = new ComputeBuffer(this.NumberToSpawn * this.GPUNeighborhoodCount, 4);
        }
    }

    void OnDisable() {
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
        this.kernel2n = this.NeighborhoodShader.FindKernel("Kernel2n");

        this.treeNeighborsTexture = new RenderTexture(this.NumberToSpawn, this.NumberToSpawn, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
        {
            enableRandomWrite = true
        };
        this.treeNeighborsTexture.Create();

        this.neighborhood = new int[this.NumberToSpawn * this.GPUNeighborhoodCount];
        for (int i = 0; i < this.neighborhood.Length; i++) {
            this.neighborhood[i] = -1;
        }

        this.NeighborhoodShader.SetInt("numberToSpawn", this.NumberToSpawn);
        this.NeighborhoodShader.SetInt("neighborhoodCount", this.GPUNeighborhoodCount);
    }

    private void SetupShaderGPUOctree() {
        this.octreeLeafsKernel = this.NeighborhoodShader.FindKernel("OctreeLeafsKernel");
        this.octreeNeighborsKernel = this.NeighborhoodShader.FindKernel("OctreeNeighborsKernel");

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
        this.treeLeafsTexture = new RenderTexture(this.treeLeafsCount, this.NumberToSpawn, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
        {
            enableRandomWrite = true
        };
        this.treeLeafsTexture.Create();

        this.treeNeighborsTexture = new RenderTexture(this.NumberToSpawn, this.NumberToSpawn, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
        {
            enableRandomWrite = true
        };
        this.treeNeighborsTexture.Create();

        this.neighborhood = new int[this.NumberToSpawn * this.GPUNeighborhoodCount];
        for (int i = 0; i < this.neighborhood.Length; i++) {
            this.neighborhood[i] = -1;
        }

        this.NeighborhoodShader.SetInt("numberToSpawn", this.NumberToSpawn);
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

        this.NeighborhoodShader.SetTexture(this.octreeLeafsKernel, "Octree", this.octreeTexture);
        this.NeighborhoodShader.SetTexture(this.octreeNeighborsKernel, "Octree", this.octreeTexture);
    }

    private void DispatchGPU2n() {
        Vector3[] positions = new Vector3[this.NumberToSpawn];
        Vector3[] directions = new Vector3[this.NumberToSpawn];
        float[] perceptionRadiuses = new float[this.NumberToSpawn];
        float[] horizontalFOVs = new float[this.NumberToSpawn];
        float[] verticalFOVs = new float[this.NumberToSpawn];

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

        this.NeighborhoodShader.SetBuffer(this.kernel2n, "Position", this.positionBuffer);
        this.NeighborhoodShader.SetBuffer(this.kernel2n, "Direction", this.directionBuffer);
        this.NeighborhoodShader.SetBuffer(this.kernel2n, "PerceptionRadius", this.perceptionRadiusBuffer);
        this.NeighborhoodShader.SetBuffer(this.kernel2n, "HorizontalFOV", this.horizontalFOVBuffer);
        this.NeighborhoodShader.SetBuffer(this.kernel2n, "VerticalFOV", this.verticalFOVBuffer);
        this.NeighborhoodShader.SetTexture(this.kernel2n, "TreeNeighbors", this.treeNeighborsTexture);
        this.NeighborhoodShader.SetBuffer(this.kernel2n, "Neighborhood", this.neighborhoodBuffer);

        this.NeighborhoodShader.Dispatch(this.kernel2n, this.NumberToSpawn / 64, 1, 1);

        AsyncGPUReadback.Request(this.neighborhoodBuffer, this.SetNeighborhood);
    }

    private void BuildGPUOctreeLeafs() {
        float xmin = float.MaxValue;
        float xmax = float.MinValue;
        float ymin = float.MaxValue;
        float ymax = float.MinValue;
        float zmin = float.MaxValue;
        float zmax = float.MinValue;

        Vector3[] positions = new Vector3[this.NumberToSpawn];
        Vector3[] directions = new Vector3[this.NumberToSpawn];
        float[] perceptionRadiuses = new float[this.NumberToSpawn];
        float[] horizontalFOVs = new float[this.NumberToSpawn];
        float[] verticalFOVs = new float[this.NumberToSpawn];

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

        this.NeighborhoodShader.SetBuffer(this.octreeLeafsKernel, "Position", this.positionBuffer);
        this.NeighborhoodShader.SetTexture(this.octreeLeafsKernel, "TreeLeafs", this.treeLeafsTexture);


        this.NeighborhoodShader.SetBuffer(this.octreeNeighborsKernel, "Position", this.positionBuffer);
        this.NeighborhoodShader.SetBuffer(this.octreeNeighborsKernel, "Direction", this.directionBuffer);
        this.NeighborhoodShader.SetBuffer(this.octreeNeighborsKernel, "PerceptionRadius", this.perceptionRadiusBuffer);
        this.NeighborhoodShader.SetBuffer(this.octreeNeighborsKernel, "HorizontalFOV", this.horizontalFOVBuffer);
        this.NeighborhoodShader.SetBuffer(this.octreeNeighborsKernel, "VerticalFOV", this.verticalFOVBuffer);
        this.NeighborhoodShader.SetTexture(this.octreeNeighborsKernel, "TreeLeafs", this.treeLeafsTexture);
        this.NeighborhoodShader.SetTexture(this.octreeNeighborsKernel, "TreeNeighbors", this.treeNeighborsTexture);
        this.NeighborhoodShader.SetBuffer(this.octreeNeighborsKernel, "Neighborhood", this.neighborhoodBuffer);


        this.NeighborhoodShader.Dispatch(this.octreeLeafsKernel, this.NumberToSpawn / 64, 1, 1);
        this.NeighborhoodShader.Dispatch(this.octreeNeighborsKernel, this.NumberToSpawn / 64, 1, 1);

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