using System;
using UnityEngine;

public class EEAndNode : EENode {

    private Action<EENode> OnAddConnection;
    private Action<EENode> OnDelete;
    private GUIStyle textStyle;

    public EEAndNode(AndNode andNode, Action<EENode> OnAddConnection, Action<EENode> OnDelete, Action<EENode> OnNodeClicked): 
    base(new Rect(andNode.x, andNode.y, 80, 35), andNode, OnNodeClicked) {
        this.OnAddConnection = OnAddConnection;
        this.OnDelete = OnDelete;
        this.AddGenericMenuItem(new GenericMenuItem(new GUIContent("Make Transition"), () => this.OnAddConnection(this)));
        this.AddGenericMenuItem(new GenericMenuItem(new GUIContent("Delete"), () => this.OnDelete(this)));
        this.textStyle = new GUIStyle();
        this.textStyle.fontSize = 20;
        this.textStyle.alignment = TextAnchor.MiddleCenter;
        this.textStyle.normal.textColor = EETheme.LightColor;
    }

    public EEAndNode(Vector2 pos, Action<EENode> OnAddConnection, Action<EENode> OnDelete, Action<EENode> OnNodeClicked): 
    this(new AndNode(pos.x, pos.y), OnAddConnection, OnDelete, OnNodeClicked) {
        FuzzyRules.FuzzyController.AddAndNode(FuzzyRules.Drive, (AndNode)this.GetNode());
    }

    public override void Move(Vector2 delta) {
        base.Move(delta);
        FuzzyRules.FuzzyController.SetAndNode(FuzzyRules.Drive, this.GetNode().guid, (AndNode)this.GetNode());
    }

    public override void Draw() {
        base.Draw();

        GUILayout.BeginArea(this.GetRect());

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginVertical();

        GUILayout.Label("AND", this.textStyle);

        GUILayout.EndVertical();

        GUILayout.Space(5);

        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.EndArea();
    }

}