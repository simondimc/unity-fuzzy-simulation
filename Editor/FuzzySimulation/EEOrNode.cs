using System;
using UnityEngine;

public class EEOrNode : EENode {

    private Action<EENode> OnAddConnection;
    private Action<EENode> OnDelete;
    private GUIStyle textStyle;

    public EEOrNode(OrNode orNode, Action<EENode> OnAddConnection, Action<EENode> OnDelete, Action<EENode> OnNodeClicked): 
    base(new Rect(orNode.x, orNode.y, 80, 35), orNode, OnNodeClicked) {
        this.OnAddConnection = OnAddConnection;
        this.OnDelete = OnDelete;
        this.AddGenericMenuItem(new GenericMenuItem(new GUIContent("Make Transition"), () => this.OnAddConnection(this)));
        this.AddGenericMenuItem(new GenericMenuItem(new GUIContent("Delete"), () => this.OnDelete(this)));
        this.textStyle = new GUIStyle();
        this.textStyle.fontSize = 20;
        this.textStyle.alignment = TextAnchor.MiddleCenter;
        this.textStyle.normal.textColor = EETheme.LightColor;
    }

    public EEOrNode(Vector2 pos, Action<EENode> OnAddConnection, Action<EENode> OnDelete, Action<EENode> OnNodeClicked):
    this(new OrNode(pos.x, pos.y), OnAddConnection, OnDelete, OnNodeClicked) {
        FuzzyRules.FuzzyController.AddOrNode(FuzzyRules.Drive, (OrNode)this.GetNode());
    }

    public override void Move(Vector2 delta) {
        base.Move(delta);
        FuzzyRules.FuzzyController.SetOrNode(FuzzyRules.Drive, this.GetNode().guid, (OrNode)this.GetNode());
    }

    public override void Draw() {
        base.Draw();

        GUILayout.BeginArea(this.GetRect());

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginVertical();

        GUILayout.Label("OR", this.textStyle);

        GUILayout.EndVertical();

        GUILayout.Space(5);

        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.EndArea();
    }

}