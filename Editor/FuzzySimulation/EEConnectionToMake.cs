using UnityEditor;
using UnityEngine;

public class EEConnectionToMake : EditorElement {

    private EENode node1;

    public EEConnectionToMake(EENode node1) {
        this.node1 = node1;
    }

    public override void Draw() {
        Handles.color = EETheme.AccentColor2;
        Handles.DrawAAPolyLine(
            3.0f,
            new Vector3[] {
                new Vector3(this.node1.GetRect().x + this.node1.GetRect().width / 2, this.node1.GetRect().y + this.node1.GetRect().height / 2, 0),
                new Vector3(Event.current.mousePosition.x, Event.current.mousePosition.y, 0)
            }
        );

        Vector3 node1center = new Vector3(
            this.node1.GetRect().x + this.node1.GetRect().width / 2,
            this.node1.GetRect().y + this.node1.GetRect().height / 2,
            0
        );
        Vector3 node2center = new Vector3(
            Event.current.mousePosition.x,
            Event.current.mousePosition.y,
            0
        );

        float z = 5;
        float dx = node2center.x - node1center.x;
        float dy = node2center.y - node1center.y;
        float d = Mathf.Sqrt(dx * dx + dy * dy);

        float alpha = Mathf.Atan2(dy, dx);
        float beta = Mathf.Atan2(dx, dy);

        Vector3 a = new Vector3(
            node1center.x + 0.5f * (node2center.x - node1center.x),
            node1center.y + 0.5f * (node2center.y - node1center.y),
            0
        );
        Vector3 b = new Vector3(
            a.x - (Mathf.Cos(beta) * z),
            a.y + (Mathf.Sin(beta) * z),
            0
        );
        Vector3 c = new Vector3(
            node1center.x + 0.5f * (node2center.x - node1center.x) + (Mathf.Cos(alpha) * z * 2),
            node1center.y + 0.5f * (node2center.y - node1center.y) + (Mathf.Sin(alpha) * z * 2),
            0
        );
        Vector3 e = new Vector3(
            a.x + (Mathf.Cos(beta) * z),
            a.y - (Mathf.Sin(beta) * z),
            0
        );

        Handles.DrawAAConvexPolygon(
            new Vector3[] {
                a, b, c, e
            }
        );

        GUI.changed = true;
    }

    public override void ProcessEvent(EEEvent e) {}

    protected override bool EventOverlapCheck(EEEvent e) {
        return false;
    }

    public override int Layer() {
        return 25;
    }

    public EENode GetNode1() {
        return this.node1;
    }

}