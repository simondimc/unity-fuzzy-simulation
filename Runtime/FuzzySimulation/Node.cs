
[System.Serializable]
public class Node {

    public string guid;
    public float x;
    public float y;

    public Node(float x, float y) {
        this.x = x;
        this.y = y;
        this.guid = System.Guid.NewGuid().ToString();
    }

}