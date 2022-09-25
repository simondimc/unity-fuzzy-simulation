using UnityEngine;

public abstract class EditorElement {

    private bool eventOnOverlapedElement = false;

    public abstract void Draw();
    public abstract void ProcessEvent(EEEvent e);
    public abstract int Layer();
    protected abstract bool EventOverlapCheck(EEEvent e);

    public void EventOverlap(EEEvent e) {
        bool eventOverlapCheck = this.EventOverlapCheck(e);
        this.eventOnOverlapedElement = eventOverlapCheck && e.Used();
        if (eventOverlapCheck) e.VirtualUse();
    }

    public void DrawAfterOverlapCheck() {
        if (this.eventOnOverlapedElement) GUI.enabled = false;
        this.Draw();
        GUI.enabled = true;
    }

}