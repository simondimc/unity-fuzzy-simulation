
[System.Serializable]
public class Variable {

    public string guid;
    public string name;
    public float upperBound;
    public float lowerBound;

    public Variable(string name, float lowerBound, float upperBound) {
        this.guid = System.Guid.NewGuid().ToString();
        this.name = name;
        this.upperBound = upperBound;
        this.lowerBound = lowerBound;
    }

    public Variable(System.Guid guid, string name, float lowerBound, float upperBound) {
        this.guid = guid.ToString();
        this.name = name;
        this.upperBound = upperBound;
        this.lowerBound = lowerBound;
    }

    public Variable Copy() {
        return new Variable(new System.Guid(this.guid), this.name, this.lowerBound, this.upperBound);
    }

    public Variable CopyNewGuid() {
        return new Variable(System.Guid.NewGuid(), this.name, this.lowerBound, this.upperBound);
    }

}