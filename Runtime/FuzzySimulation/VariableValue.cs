
using System.Collections.Generic;

[System.Serializable]
public class VariableValue {
    
    public string guid;
    public string name;
    public string variableGuid;
    public List<GraphPoint> graphPoints;
    public List<GraphSample> graphSamples;

    public VariableValue() {
        this.guid = System.Guid.NewGuid().ToString();
        this.name = "";
        this.graphPoints = new List<GraphPoint>();
        this.graphSamples = new List<GraphSample>();
    }

    public VariableValue(System.Guid guid, string name, string variableGuid) {
        this.guid = guid.ToString();
        this.name = name;
        this.variableGuid = variableGuid;
        this.graphPoints = new List<GraphPoint>();
        this.graphSamples = new List<GraphSample>();
    }

    public VariableValue(System.Guid guid, string name, string variableGuid, List<GraphPoint> graphPoints, List<GraphSample> graphSamples) {
        this.guid = guid.ToString();
        this.name = name;
        this.variableGuid = variableGuid;
        this.graphPoints = graphPoints;
        this.graphSamples = graphSamples;
    }

    public VariableValue Copy() {
        return new VariableValue(new System.Guid(this.guid), this.name, this.variableGuid, this.graphPoints, this.graphSamples);
    }

    public VariableValue CopyNewGuid() {
        return new VariableValue(System.Guid.NewGuid(), this.name, this.variableGuid, this.graphPoints, this.graphSamples);
    }

}