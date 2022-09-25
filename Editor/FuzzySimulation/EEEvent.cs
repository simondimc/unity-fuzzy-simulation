using UnityEngine;

public class EEEvent {

    private Event e;
    private bool used;

    public EEEvent(Event e) {
        this.e = e;
        this.used = false;
    }

    public void RealUse() {
        this.e.Use();
    }

    public void VirtualUse() {
        this.used = true;
    }

    public bool Used() {
        return this.used;
    }

    public Event GetEvent() {
        return this.e;
    }

}