using System.Collections.Generic;

public enum NodeTreeType {
    GraphNode,
    AndNode,
    OrNode,
    NotNode
}

public class NodeTree {

    public Node node;
    public List<NodeTree> children;
    public NodeTreeType type;

    public NodeTree(Node node, NodeTreeType type) {
        this.node = node;
        this.type = type;
        this.children = new List<NodeTree>();
    }

}
