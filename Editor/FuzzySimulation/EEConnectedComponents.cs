using System;
using UnityEditor;
using UnityEngine;

public class EEConnectedComponents : EditorElement {

    private Rect rect;
    private GUIStyle style;
    private GUIStyle areaStyle;
    private int numOfComponents;
    private int component;
    private Action<int> OnFocusComponent;

    public EEConnectedComponents(float x, float y, float width, float height, Action<int> OnFocusComponent) {
        this.rect = new Rect(x, y, width, height);
        this.style = new GUIStyle();
        this.style.normal.background = EEUtils.ColorTexture(EETheme.AccentColor1);
        this.areaStyle = new GUIStyle();
        this.areaStyle.padding = new RectOffset(5, 5, 5, 5);
        this.numOfComponents = 0;
        this.component = 0;
        this.OnFocusComponent = OnFocusComponent;
    }

    public override void Draw() {
        GUI.Box(this.rect, "", this.style);

        GUILayout.BeginArea(this.rect, this.areaStyle);

        GUILayout.BeginHorizontal();

        GUILayout.Label("CC:", GUILayout.Width(45));

        EditorGUILayout.SelectableLabel((this.component + 1) + "/" + this.numOfComponents, EETheme.TextFieldStyle, GUILayout.Height(20));

        GUILayout.Space(5);

        if (GUILayout.Button("<", GUILayout.Width(30))) {
            if (this.numOfComponents > 0) {
                this.component = this.Mod((this.component - 1), this.numOfComponents);
                this.OnFocusComponent(this.component);
            }
        }

        if (GUILayout.Button(">", GUILayout.Width(30))) {
            if (this.numOfComponents > 0) {
                this.component = this.Mod((this.component + 1), this.numOfComponents);
                this.OnFocusComponent(this.component);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private int Mod(int a, int n) {
        return ((a % n) + n) % n;
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

    public void SetNumOfComponents(int numOfComponents) {
        this.numOfComponents = numOfComponents;
        if (this.component > numOfComponents - 1) {
            this.component = numOfComponents - 1;
        }
    }

    public void SetComponent(int component) {
        this.component = component;
        if (this.component > numOfComponents - 1) {
            this.component = numOfComponents - 1;
        }
    }

}