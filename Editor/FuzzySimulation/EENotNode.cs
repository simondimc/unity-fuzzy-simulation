using System;
using UnityEngine;

public class EENotNode : EENode {

    private Action<EENode> OnAddConnection;
    private Action<EENode> OnDelete;
    private GUIStyle textStyle;

    public EENotNode(NotNode notNode, Action<EENode> OnAddConnection, Action<EENode> OnDelete, Action<EENode> OnNodeClicked): 
    base(new Rect(notNode.x, notNode.y, 80, 35), notNode, OnNodeClicked) {
        this.OnAddConnection = OnAddConnection;
        this.OnDelete = OnDelete;
        this.AddGenericMenuItem(new GenericMenuItem(new GUIContent("Make Transition"), () => this.OnAddConnection(this)));
        this.AddGenericMenuItem(new GenericMenuItem(new GUIContent("Delete"), () => this.OnDelete(this)));
        this.textStyle = new GUIStyle();
        this.textStyle.fontSize = 20;
        this.textStyle.alignment = TextAnchor.MiddleCenter;
        this.textStyle.normal.textColor = EETheme.LightColor;
    }

    public EENotNode(Vector2 pos, Action<EENode> OnAddConnection, Action<EENode> OnDelete, Action<EENode> OnNodeClicked):
    this(new NotNode(pos.x, pos.y), OnAddConnection, OnDelete, OnNodeClicked) {
        FuzzyRules.FuzzyController.AddNotNode(FuzzyRules.Drive, (NotNode)this.GetNode());
    }

    public override void Move(Vector2 delta) {
        base.Move(delta);
        FuzzyRules.FuzzyController.SetNotNode(FuzzyRules.Drive, this.GetNode().guid, (NotNode)this.GetNode());
    }

    public override void Draw() {
        base.Draw();

        GUILayout.BeginArea(this.GetRect());

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginVertical();

        GUILayout.Label("NOT", this.textStyle);

        GUILayout.EndVertical();

        GUILayout.Space(5);

        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.EndArea();
    }

}