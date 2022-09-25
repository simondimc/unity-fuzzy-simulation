using System.Collections.Generic;

[System.Serializable]
public class Drive {

    public string guid;
    public string name;
    public bool isOpen;
    public bool isMinView;
    public List<Connection> connections = new List<Connection>();
    public List<GraphNode> inputGraphNodes = new List<GraphNode>();
    public List<GraphNode> outputGraphNodes = new List<GraphNode>();
    public List<AndNode> andNodes = new List<AndNode>();
    public List<OrNode> orNodes = new List<OrNode>();
    public List<NotNode> notNodes = new List<NotNode>();

    public Drive(string name) {
        this.guid = System.Guid.NewGuid().ToString();
        this.name = name;
        this.isOpen = false;
        this.isMinView = false;
    }

    public Drive(System.Guid guid, string name) {
        this.guid = guid.ToString();
        this.name = name;
        this.isOpen = false;
        this.isMinView = false;
    }

    public Drive Copy() {
        return new Drive(new System.Guid(this.guid), this.name);
    }

    public Drive CopyNewGuid() {
        return new Drive(System.Guid.NewGuid(), this.name);
    }

}
