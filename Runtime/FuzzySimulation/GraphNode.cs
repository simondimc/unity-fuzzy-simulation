
[System.Serializable]
public class GraphNode : Node {

    public string variableGuid;
    public string variableValueGuid;

    public GraphNode(float x, float y): 
    base(x, y) {}

    public GraphNode(float x, float y, string variableGuid, string variableValueGuid): 
    base(x, y) {
        this.variableGuid = variableGuid;
        this.variableValueGuid = variableValueGuid;
    }

}
