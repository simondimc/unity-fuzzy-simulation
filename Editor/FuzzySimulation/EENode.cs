using System;
using UnityEngine;

public class EENode : EEDraggableFrame {

    private Node node;
    protected Action<EENode> OnNodeClicked;

    public EENode(Rect rect, Node node, Action<EENode> OnNodeClicked): 
    base(rect) {
        this.node = node;
        this.OnNodeClicked = OnNodeClicked;
    }

    public override void ProcessEvent(EEEvent e) {
        base.ProcessEvent(e);
        
        switch (e.GetEvent().type) {
            case EventType.MouseDown:
                if (e.GetEvent().button == 0) {
                    if (this.GetFrameRect().Contains(e.GetEvent().mousePosition)) {
                        this.OnNodeClicked(this);
                        e.VirtualUse();
                    }
                }
                break;
        }
    }

    public override int Layer() {
        return 50;
    }

    public override void Move(Vector2 delta) {
        base.Move(delta);
        this.node.x += delta.x;
        this.node.y += delta.y;
    }

    public Node GetNode() {
        return this.node;
    }

}