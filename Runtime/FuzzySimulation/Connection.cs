
[System.Serializable]
public class Connection {

    public string node1Guid;
    public string node2Guid;

    public Connection(string node1Guid, string node2Guid) {
        this.node1Guid = node1Guid;
        this.node2Guid = node2Guid;
    }

}
