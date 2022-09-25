using UnityEngine;

public class EEDraggableFrame : EESelectableFrame {

    private bool isDragged;
    private Vector2 drag;

    public EEDraggableFrame(Rect rect): 
    base(rect) {
        this.isDragged = false;
    }

    public virtual void Drag(Vector2 delta) {        
        this.Move(delta);
    }

    public override void ProcessEvent(EEEvent e) {
        base.ProcessEvent(e);
        
        this.drag = Vector2.zero;

        switch (e.GetEvent().type) {
            case EventType.MouseDown:
                if (e.GetEvent().button == 0) {
                    if (this.GetFrameRect().Contains(e.GetEvent().mousePosition)) {
                        this.isDragged = true;
                        GUI.changed = true;
                    } else {
                        GUI.changed = true;
                    }
                }
                break;
            case EventType.MouseUp:
                this.isDragged = false;
                break;
            case EventType.MouseDrag:
                if (e.GetEvent().button == 0 && this.isDragged) {
                    this.Drag(e.GetEvent().delta);
                    e.RealUse();
                }
                break;
        }
    }

}