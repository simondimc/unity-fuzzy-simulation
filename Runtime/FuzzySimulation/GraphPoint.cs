
[System.Serializable]
public class GraphPoint {

    public float x;
    public float y;
    public float inTangent;
    public float outTangent;
    public float inWeight;
    public float outWeight;
    public int weightedMode;
    public int leftTangentMode;
    public int rightTangentMode;

    public GraphPoint(float x, float y, float inTangent, float outTangent, float inWeight, float outWeight, int weightedMode, int leftTangentMode, int rightTangentMode) {
        this.x = x;
        this.y = y;
        this.inTangent = inTangent;
        this.outTangent = outTangent;
        this.inWeight = inWeight;
        this.outWeight = outWeight;
        this.weightedMode = weightedMode;
        this.leftTangentMode = leftTangentMode;
        this.rightTangentMode = rightTangentMode;
    }

}
