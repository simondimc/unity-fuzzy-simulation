using System;
using UnityEditor;
using UnityEngine;

public class EEConnection : EditorElement {

    private Connection connection;
    private EENode node1;
    private EENode node2;
    private Action<EEConnection> OnDeleteConnection;
    private bool isSelected;

    public EEConnection(Connection connection, Action<EEConnection> setNodes, Action<EEConnection> OnDeleteConnection) {
        this.connection = connection;
        setNodes(this);
        this.CommonConstructor(OnDeleteConnection);
    }

    public EEConnection(EENode node1, EENode node2, Action<EEConnection> OnDeleteConnection) {
        this.node1 = node1;
        this.node2 = node2;
        this.connection = new Connection(this.node1.GetNode().guid, this.node2.GetNode().guid);
        FuzzyRules.FuzzyController.AddConnection(FuzzyRules.Drive, this.connection);
        this.CommonConstructor(OnDeleteConnection);
    }

    private void CommonConstructor(Action<EEConnection> OnDeleteConnection) {
        this.OnDeleteConnection = OnDeleteConnection;
    }

    public void SetNode1(EENode node) {
        if (node.GetNode().guid == this.connection.node1Guid) {
            this.node1 = node;
        }
    }

    public void SetNode2(EENode node) {
        if (node.GetNode().guid == this.connection.node2Guid) {
            this.node2 = node;
        }
    }  

    public override void Draw() {
        if (this.isSelected) {
            Handles.color = EETheme.AccentColor3;
        } else {
            Handles.color = EETheme.AccentColor2;
        }
        
        Handles.DrawAAPolyLine(
            3.0f,
            new Vector3[] {
                new Vector3(this.node1.GetBorderRect().x + this.node1.GetBorderRect().width / 2, this.node1.GetBorderRect().y + this.node1.GetBorderRect().height / 2, 0),
                new Vector3(this.node2.GetBorderRect().x + this.node2.GetBorderRect().width / 2, this.node2.GetBorderRect().y + this.node2.GetBorderRect().height / 2, 0)
            }
        );

        Vector3 node1center = new Vector3(
            this.node1.GetBorderRect().x + this.node1.GetBorderRect().width / 2,
            this.node1.GetBorderRect().y + this.node1.GetBorderRect().height / 2,
            0
        );
        Vector3 node2center = new Vector3(
            this.node2.GetBorderRect().x + this.node2.GetBorderRect().width / 2,
            this.node2.GetBorderRect().y + this.node2.GetBorderRect().height / 2,
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
    }

    public override void ProcessEvent(EEEvent e) {
        switch (e.GetEvent().type) {
            case EventType.MouseDown:
                if (e.GetEvent().button == 0 || e.GetEvent().button == 1) {
                    if (!e.Used() &&  Utils.DistanceLinePoint(new Vector2(this.node1.GetBorderRect().x + this.node1.GetBorderRect().width / 2, this.node1.GetBorderRect().y + this.node1.GetBorderRect().height / 2), new Vector2(this.node2.GetBorderRect().x + this.node2.GetBorderRect().width / 2, this.node2.GetBorderRect().y + this.node2.GetBorderRect().height / 2), e.GetEvent().mousePosition) < 5) {
                        this.isSelected = true;
                        GUI.changed = true;
                        e.RealUse();
                    } else {
                        this.isSelected = false;
                        GUI.changed = true;
                    }
                }
                if (e.GetEvent().button == 1 && isSelected && Utils.DistanceLinePoint(new Vector2(this.node1.GetBorderRect().x + this.node1.GetBorderRect().width / 2, this.node1.GetBorderRect().y + this.node1.GetBorderRect().height / 2), new Vector2(this.node2.GetBorderRect().x + this.node2.GetBorderRect().width / 2, this.node2.GetBorderRect().y + this.node2.GetBorderRect().height / 2), e.GetEvent().mousePosition) < 5) {
                    ProcessContextMenu();
                    e.RealUse();
                }
                break;
        }
    }

    protected override bool EventOverlapCheck(EEEvent e) {
        return false;
    }

    private void ProcessContextMenu() {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Delete"), false, () => this.OnDeleteConnection(this));
        genericMenu.ShowAsContext();
    }

    public override int Layer() {
        return 25;
    }

    public EENode GetNode1() {
        return this.node1;
    }
    
    public EENode GetNode2() {
        return this.node2;
    }

    public Connection GetConnection() {
        return this.connection;
    }

}