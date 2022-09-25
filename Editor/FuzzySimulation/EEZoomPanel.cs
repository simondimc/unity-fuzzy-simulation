using UnityEditor;
using UnityEngine;
using System;

public class EEZoomPanel : EditorElement {

    private Rect rect;
    private GUIStyle style;
    private GUIStyle areaStyle;
    private bool toggle;

    public EEZoomPanel(float x, float y, float width, float height) {
        this.rect = new Rect(x, y, width, height);
        this.style = new GUIStyle();
        this.style.normal.background = EEUtils.ColorTexture(EETheme.AccentColor1);
        this.areaStyle = new GUIStyle();
        this.areaStyle.padding = new RectOffset(5, 5, 5, 5);
    }

    public override void Draw() {
        GUI.Box(this.rect, "", this.style);

        GUILayout.BeginArea(this.rect, this.areaStyle);

        GUILayout.BeginHorizontal();

        GUILayout.Label("Min View:", GUILayout.Width(60));

        GUILayout.Space(5);

        FuzzyRules.Drive.isMinView = EditorGUILayout.Toggle(FuzzyRules.Drive.isMinView);

        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }
    
    public override void ProcessEvent(EEEvent e) {
        switch (e.GetEvent().type) {
            case EventType.MouseDown:
                if (e.GetEvent().button == 0 || e.GetEvent().button == 1) {
                    if (this.rect.Contains(e.GetEvent().mousePosition)) {
                        e.RealUse();
                    }
                }
                break;
            case EventType.MouseDrag:
                if (e.GetEvent().button == 0) {
                    if (this.rect.Contains(e.GetEvent().mousePosition)) {
                        e.RealUse();
                    }
                }
                break;
        }
    }

    protected override bool EventOverlapCheck(EEEvent e) {
        switch (e.GetEvent().type) {
            case EventType.MouseDown:
                if (e.GetEvent().button == 0 || e.GetEvent().button == 1) {
                    if (this.rect.Contains(e.GetEvent().mousePosition)) {
                        return true;
                    }
                }
                break;
        }
        return false;
    }

    public override int Layer() {
        return 100;
    }

}