using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Fuzzy Controller", menuName = "Fuzzy Controller", order = 1)]
public class FuzzyController : ScriptableObject {
        
    [SerializeField] private List<Variable> inputVariables = new List<Variable>();
    [SerializeField] private List<Variable> outputVariables = new List<Variable>();
    [SerializeField] private List<VariableValue> variableValues = new List<VariableValue>();
    [SerializeField] private List<Drive> drives = new List<Drive>();

    private Dictionary<string, Variable> variablesMap;
    private Dictionary<string, VariableValue> variableValuesMap;
    private Dictionary<string, List<Sample>> variableSamples;
    private Dictionary<string, Sample> variableValueSampleMap;

    private List<NodeTree> trees;
    private Dictionary<string, NodeTree> guidTree;
    private Dictionary<string, float?> variableGuidValue;
    private Dictionary<string, Variable> variableNameVariable;

    private Dictionary<string, string> driveNameDrive;
    private List<string> disabledDrives;

    private float yStep = 1.0f / 100;
    private Dictionary<string, List<Sample>> variableValueYMinSample;
    private Dictionary<string, List<Sample>> variableValueYProdSample;

    private FuzzySetOperations fuzzySetOperations;
    private FuzzySetOperationsMode fuzzySetOperationsMode;

    public void SetFuzzySetOperationType(FuzzySetOperationsMode fuzzySetOperationsMode) {
        this.fuzzySetOperationsMode = fuzzySetOperationsMode;

        switch (fuzzySetOperationsMode) {
            case FuzzySetOperationsMode.MinimumAndMaximum:
                this.fuzzySetOperations = new MinimumMaximumFuzzySetOperations();
                break;
            case FuzzySetOperationsMode.AlgebraicProductAndSum:
                this.fuzzySetOperations = new AlgebraicProductSumFuzzySetOperations();
                break;
        }
    }

    private void Reset() {
        Drive defaultDrive = new Drive("Default");
        defaultDrive.isOpen = true;
        drives.Add(defaultDrive);
    }

    public void OnEnable() {
        this.fuzzySetOperations = new MinimumMaximumFuzzySetOperations();

        this.variableSamples = new Dictionary<string, List<Sample>>();
        this.variableGuidValue = new Dictionary<string, float?>();
        this.variableNameVariable = new Dictionary<string, Variable>();
        this.variablesMap = new Dictionary<string, Variable>();
        this.variableValuesMap = new Dictionary<string, VariableValue>();
        this.variableValueSampleMap = new Dictionary<string, Sample>();
        this.trees = new List<NodeTree>();
        this.guidTree = new Dictionary<string, NodeTree>();

        this.variableValueYMinSample = new Dictionary<string, List<Sample>>();
        this.variableValueYProdSample = new Dictionary<string, List<Sample>>();

        this.driveNameDrive = new Dictionary<string, string>();
        this.disabledDrives = new List<string>();

        foreach (Drive drive in this.drives) {
            this.driveNameDrive[drive.name] = drive.guid;
        }

        foreach (Variable inputVariable in this.inputVariables) {
            this.variablesMap.Add(inputVariable.guid, inputVariable);
            this.variableNameVariable.Add(inputVariable.name, inputVariable);
            this.variableGuidValue.Add(inputVariable.guid, null);
        }

        foreach (Variable outputVariable in this.outputVariables) {
            this.variablesMap.Add(outputVariable.guid, outputVariable);
            this.variableNameVariable.Add(outputVariable.name, outputVariable);
            this.variableGuidValue.Add(outputVariable.guid, null);
        }

        foreach (VariableValue variableValue in this.variableValues) {
            this.variableValuesMap[variableValue.guid] = variableValue;

            Sample sample = new Sample(
                variableValue.graphSamples.Select(s => s.x).ToArray(), 
                variableValue.graphSamples.Select(s => s.y).ToArray()
            );

            this.variableValueSampleMap[variableValue.guid] = sample;

            if (!this.variableValueYMinSample.ContainsKey(variableValue.guid) || !this.variableValueYProdSample.ContainsKey(variableValue.guid)) {
                List<Sample> minSamples = new List<Sample>();
                List<Sample> prodSamples = new List<Sample>();

                for (float i = 0.0f; i <= 1.0f; i += yStep) {
                    Sample minSample = this.variableValueSampleMap[variableValue.guid].Copy();
                    Sample prodSample = this.variableValueSampleMap[variableValue.guid].Copy();   

                    for (int j = 0; j < minSample.y.Length; j++) {
                        minSample.y[j] = Mathf.Min(minSample.y[j], i);
                        prodSample.y[j] = minSample.y[j] * i;
                    }

                    minSamples.Add(minSample);
                    prodSamples.Add(prodSample);
                }

                this.variableValueYMinSample[variableValue.guid] = minSamples;
                this.variableValueYProdSample[variableValue.guid] = prodSamples;
            }
        }

        this.SetupForPlay();
    }

    private void SetupForPlay() {
        this.trees.Clear();
        this.guidTree.Clear();

        List<Connection> connections = new List<Connection>();
        List<GraphNode> inputGraphNodes = new List<GraphNode>();
        List<GraphNode> outputGraphNodes = new List<GraphNode>();
        List<AndNode> andNodes = new List<AndNode>();
        List<OrNode> orNodes = new List<OrNode>();
        List<NotNode> notNodes = new List<NotNode>();
        Dictionary<string, NodeTree> variableValueGraphNode = new Dictionary<string, NodeTree>();

        foreach (Drive drive in this.drives) {
            if (!this.disabledDrives.Contains(drive.guid)) {
                connections.AddRange(drive.connections);
                inputGraphNodes.AddRange(drive.inputGraphNodes);
                outputGraphNodes.AddRange(drive.outputGraphNodes);
                andNodes.AddRange(drive.andNodes);
                orNodes.AddRange(drive.orNodes);
                notNodes.AddRange(drive.notNodes);
            }
        }

        foreach (GraphNode inputGraphNode in inputGraphNodes) {
            NodeTree tree = new NodeTree(inputGraphNode, NodeTreeType.GraphNode);
            this.guidTree.Add(inputGraphNode.guid, tree);
            this.trees.Add(tree);
        }

        foreach (GraphNode outputGraphNode in outputGraphNodes) {
            NodeTree tree = new NodeTree(outputGraphNode, NodeTreeType.GraphNode);
            this.guidTree.Add(outputGraphNode.guid, tree);
            this.trees.Add(tree);

            if (!variableValueGraphNode.ContainsKey(outputGraphNode.variableValueGuid)) {
                GraphNode copyNode = new GraphNode(outputGraphNode.x, outputGraphNode.y, outputGraphNode.variableGuid, outputGraphNode.variableValueGuid);
                NodeTree copyTree = new NodeTree(copyNode, NodeTreeType.GraphNode);
                variableValueGraphNode[outputGraphNode.variableValueGuid] = copyTree;
            }
        }

        foreach (AndNode andNode in andNodes) {
            NodeTree tree = new NodeTree(andNode, NodeTreeType.AndNode);
            this.guidTree.Add(andNode.guid, tree);
            this.trees.Add(tree);
        }

        foreach (OrNode orNode in orNodes) {
            NodeTree tree = new NodeTree(orNode, NodeTreeType.OrNode);
            this.guidTree.Add(orNode.guid, tree);
            this.trees.Add(tree);
        }

        foreach (NotNode notNode in notNodes) {
            NodeTree tree = new NodeTree(notNode, NodeTreeType.NotNode);
            this.guidTree.Add(notNode.guid, tree);
            this.trees.Add(tree);
        }
    
        foreach (NodeTree tree in variableValueGraphNode.Values) {
            this.guidTree.Add(tree.node.guid, tree);
            this.trees.Add(tree);
        }

        foreach (Connection connection in connections) {
            NodeTree tree1 = this.guidTree[connection.node1Guid];
            NodeTree tree2 = this.guidTree[connection.node2Guid];

            if (tree2.type == NodeTreeType.GraphNode && variableValueGraphNode.ContainsKey(((GraphNode)tree2.node).variableValueGuid)) {
                trees.Remove(tree2);
                tree2 = variableValueGraphNode[((GraphNode)tree2.node).variableValueGuid];
            }

            tree2.children.Add(tree1);
            trees.Remove(tree1);
        }

        /*
        foreach (NodeTree tree in this.trees) {
            PrintTree(tree, 0);
        }
        */
    }

    public void SetDriveEnabled(string drive, bool enabled) {
        if (enabled) {
            this.disabledDrives.Remove(this.driveNameDrive[drive]);
        } else {
            this.disabledDrives.Add(this.driveNameDrive[drive]);
        }
        this.SetupForPlay();
    }

    void PrintTree(NodeTree tree, int level) {
        string spaces = "".PadRight(level * 8, ' ');
        Debug.Log(spaces + tree.node.guid);
        foreach (NodeTree child in tree.children) {
            PrintTree(child, level + 1);
        }
    }

    public bool SetValue(string variableName, float value) {
        if (this.variableNameVariable.ContainsKey(variableName)) {
            Variable variable = this.variableNameVariable[variableName];
            if (variable != null && value >= variable.lowerBound && value <= variable.upperBound) {
                this.variableGuidValue[variable.guid] = value;
                return true;
            }
        }
        return false;
    }

    public void Step() {
        this.variableSamples.Clear();

        foreach (NodeTree tree in this.trees) {
            if (tree.type != NodeTreeType.GraphNode) continue;

            float? v = null;
            v = this.fuzzySetOperations.Union(tree.children, CalcTree);
            if (v == null) continue;

            GraphNode graphNode = (GraphNode)tree.node;

            Sample sample = null;

            switch (this.fuzzySetOperationsMode) {
                case FuzzySetOperationsMode.MinimumAndMaximum:
                    sample = this.variableValueYMinSample[graphNode.variableValueGuid][Mathf.RoundToInt(v.Value / yStep)];
                    break;
                case FuzzySetOperationsMode.AlgebraicProductAndSum:
                    sample = this.variableValueYProdSample[graphNode.variableValueGuid][Mathf.RoundToInt(v.Value / yStep)];
                    break;
            }

            if (sample != null) {
                if (!variableSamples.ContainsKey(graphNode.variableGuid)) {
                    variableSamples[graphNode.variableGuid] = new List<Sample>();
                }
                variableSamples[graphNode.variableGuid].Add(sample);
            }
        }

        foreach (string guid in variableSamples.Keys) {

            float[] x = variableSamples[guid][0].x;
            float[] y = new float[variableSamples[guid][0].y.Length]; 
            variableSamples[guid][0].y.CopyTo(y, 0);

            foreach (Sample sample in variableSamples[guid].GetRange(1, variableSamples[guid].Count - 1)) {
                for (int i = 0; i < sample.y.Length; i++) {
                    switch (this.fuzzySetOperationsMode) {
                        case FuzzySetOperationsMode.MinimumAndMaximum:
                            if (sample.y[i] > y[i]) {
                                y[i] = sample.y[i];
                            }
                            break;
                        case FuzzySetOperationsMode.AlgebraicProductAndSum:
                            y[i] = y[i] + sample.y[i] - y[i] * sample.y[i];
                            break;
                    }
                }
            }
            this.variableGuidValue[guid] = this.CenterOfGravity(x, y);
        }
    }

    private float? CenterOfGravity(float[] x, float[] y) {
        float cog = 0;
        float sum_y = 0;
        for (int i = 0; i < x.Length; i++) {
            cog += x[i] * y[i];
            sum_y += y[i];
        }
        if (sum_y == 0) return null;
        cog /= sum_y;
        return cog;
    }

    private float? CalcTree(NodeTree tree) {
        if (tree.type == NodeTreeType.GraphNode) {
            GraphNode graphNode = (GraphNode)tree.node;

            Variable variable = this.variablesMap[graphNode.variableGuid];

            float? value = this.variableGuidValue[graphNode.variableGuid];

            if (value == null) return null;

            Sample sample = this.variableValueSampleMap[graphNode.variableValueGuid];

            int i = Mathf.RoundToInt(((value.Value - variable.lowerBound) / (variable.upperBound - variable.lowerBound)) * (sample.x.Length - 1));

            float v = sample.y[i];

            if (v < 0) v = 0;
            if (v > 1) v = 1;

            return v;
        } else if (tree.type == NodeTreeType.AndNode) {
            return this.fuzzySetOperations.Intersection(tree.children, CalcTree);
        } else if (tree.type == NodeTreeType.OrNode) {
            return this.fuzzySetOperations.Union(tree.children, CalcTree);
        } else if (tree.type == NodeTreeType.NotNode) {
            if (tree.children.Count != 1) return null;
            float? v = CalcTree(tree.children[0]);
            if (v == null) return null;
            return 1 - v;
        } else {
            return null;
        }
    }

    public float? GetValue(string variableName) {
        if (this.variableNameVariable.ContainsKey(variableName)) {
            Variable variable = this.variableNameVariable[variableName];
            if (variable != null) {
                return this.variableGuidValue[variable.guid];
            }
        }
        return null;
    }

    public IEnumerable<Variable> GetInputVariables() {
        return this.inputVariables.AsReadOnly();
    }

    public Variable GetInputVariable(string guid) {
        int index = this.inputVariables.FindIndex(x => x.guid == guid);
        if (index < 0) return null;
        return this.inputVariables[index];
    }

    public void AddInputVariable(Variable variable) {
        this.inputVariables.Add(variable);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public void SetInputVariable(string guid, Variable variable) {
        int index = this.inputVariables.FindIndex(x => x.guid == guid);
        this.inputVariables[index] = variable;
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public bool RemoveInputVariable(Variable variable) {
        bool r = this.inputVariables.Remove(variable);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
        return r;
    }

    public IEnumerable<Variable> GetOutputVariables() {
        return this.outputVariables.AsReadOnly();
    }

    public Variable GetOutputVariable(string guid) {
        int index = this.outputVariables.FindIndex(x => x.guid == guid);
        if (index < 0) return null;
        return this.outputVariables[index];
    }

    public void AddOutputVariable(Variable variable) {
        this.outputVariables.Add(variable);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public void SetOutputVariable(string guid, Variable variable) {
        int index = this.outputVariables.FindIndex(x => x.guid == guid);
        this.outputVariables[index] = variable;
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public bool RemoveOutputVariable(Variable variable) {
        bool r = this.outputVariables.Remove(variable);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
        return r;
    }

    public IEnumerable<Variable> GetVariables() {
        return this.inputVariables.Concat(outputVariables);
    }

    public IEnumerable<VariableValue> GetInputVariableValues() {
        List<string> inputVariableGuids = this.inputVariables.Select(x => x.guid).ToList();
        return this.variableValues.Where(x => inputVariableGuids.Contains(x.variableGuid)).ToList().AsReadOnly();
    }

    public IEnumerable<VariableValue> GetOutputVariableValues() {
        List<string> outputVariableGuids = this.outputVariables.Select(x => x.guid).ToList();
        return this.variableValues.Where(x => outputVariableGuids.Contains(x.variableGuid)).ToList().AsReadOnly();
    }

    public IEnumerable<VariableValue> GetVariableValues() {
        return this.variableValues.AsReadOnly();
    }

    public VariableValue GetVariableValue(string guid) {
        int index = this.variableValues.FindIndex(x => x.guid == guid);
        if (index < 0) return null;
        return this.variableValues[index];
    }

    public void AddVariableValue(VariableValue variableValue) {
        this.variableValues.Add(variableValue);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public void SetVariableValue(string guid, VariableValue variableValue) {
        int index = this.variableValues.FindIndex(x => x.guid == guid);
        this.variableValues[index] = variableValue;
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public bool RemoveVariableValue(VariableValue variableValue) {
        bool r = this.variableValues.Remove(variableValue);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
        return r;
    }

    public IEnumerable<Connection> GetConnections(Drive drive) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        return drive.connections.AsReadOnly();
    }

    public void AddConnection(Drive drive, Connection graphNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        drive.connections.Add(graphNode);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public bool RemoveConnection(Drive drive, Connection connection) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        bool r = drive.connections.Remove(connection);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
        return r;
    }

    public IEnumerable<GraphNode> GetInputGraphNodes(Drive drive) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        return drive.inputGraphNodes.AsReadOnly();
    }

    public void AddInputGraphNode(Drive drive, GraphNode graphNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        drive.inputGraphNodes.Add(graphNode);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public void SetInputGraphNode(Drive drive, string guid, GraphNode graphNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];

        index = drive.inputGraphNodes.FindIndex(x => x.guid == guid);
        drive.inputGraphNodes[index] = graphNode;
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public bool RemoveInputGraphNode(Drive drive, GraphNode graphNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        bool r = drive.inputGraphNodes.Remove(graphNode);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
        return r;
    }

    public IEnumerable<GraphNode> GetOutputGraphNodes(Drive drive) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        return drive.outputGraphNodes.AsReadOnly();
    }

    public void AddOutputGraphNode(Drive drive, GraphNode graphNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        drive.outputGraphNodes.Add(graphNode);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public void SetOutputGraphNode(Drive drive, string guid, GraphNode graphNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];

        index = drive.outputGraphNodes.FindIndex(x => x.guid == guid);
        drive.outputGraphNodes[index] = graphNode;
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public bool RemoveOutputGraphNode(Drive drive, GraphNode graphNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        bool r = drive.outputGraphNodes.Remove(graphNode);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
        return r;
    }

    public IEnumerable<AndNode> GetAndNodes(Drive drive) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        return drive.andNodes.AsReadOnly();
    }

    public void AddAndNode(Drive drive, AndNode andNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        drive.andNodes.Add(andNode);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public void SetAndNode(Drive drive, string guid, AndNode andNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];

        index = drive.andNodes.FindIndex(x => x.guid == guid);
        drive.andNodes[index] = andNode;
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public bool RemoveAndNode(Drive drive, AndNode andNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        bool r = drive.andNodes.Remove(andNode);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
        return r;
    }

    public IEnumerable<OrNode> GetOrNodes(Drive drive) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        return drive.orNodes.AsReadOnly();
    }

    public void AddOrNode(Drive drive, OrNode orNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        drive.orNodes.Add(orNode);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public void SetOrNode(Drive drive, string guid, OrNode orNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];

        index = drive.orNodes.FindIndex(x => x.guid == guid);
        drive.orNodes[index] = orNode;
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public bool RemoveOrNode(Drive drive, OrNode orNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        bool r = drive.orNodes.Remove(orNode);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
        return r;
    }

    public IEnumerable<NotNode> GetNotNodes(Drive drive) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        return drive.notNodes.AsReadOnly();
    }

    public void AddNotNode(Drive drive, NotNode notNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        drive.notNodes.Add(notNode);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public void SetNotNode(Drive drive, string guid, NotNode notNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];

        index = drive.notNodes.FindIndex(x => x.guid == guid);
        drive.notNodes[index] = notNode;
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public bool RemoveNotNode(Drive drive, NotNode notNode) {
        int index = this.drives.FindIndex(x => x.guid == drive.guid);
        drive = this.drives[index];
        bool r = drive.notNodes.Remove(notNode);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
        return r;
    }

    public IEnumerable<Drive> GetDrives() {
        return this.drives.AsReadOnly();
    }

    public void AddDrive(Drive drive) {
        this.drives.Add(drive);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public void SetDrive(string guid, Drive drive) {
        int index = this.drives.FindIndex(x => x.guid == guid);
        this.drives[index] = drive;
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    public bool RemoveDrive(Drive drive) {
        bool r = this.drives.Remove(drive);
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
        return r;
    }

}