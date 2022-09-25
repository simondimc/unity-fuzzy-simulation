using UnityEngine;

public abstract class EESidePanelSection : EditorElement {

    private GUIStyle sectionStyle;
    private GUIStyle headerStyle;
    private GUIStyle bodyStyle;
    private bool collapsed;
    private string title;

    public abstract void DrawBody();

    public EESidePanelSection(string title) {
        this.title = title;
        this.collapsed = false;

        this.sectionStyle = new GUIStyle();
        this.sectionStyle.margin = new RectOffset(5, 5, 5, 10);

        this.headerStyle = new GUIStyle();
        this.headerStyle.normal.background = EEUtils.ColorTexture(EETheme.HeaderColor);
        this.headerStyle.padding = new RectOffset(5, 5, 5, 5);

        this.bodyStyle = new GUIStyle();
        this.bodyStyle.normal.background = EEUtils.ColorTexture(EETheme.BodyColor);
        this.bodyStyle.padding = new RectOffset(5, 5, 5, 5);
    }

    public override void Draw() {
        GUILayout.BeginVertical(this.sectionStyle);

        GUILayout.BeginHorizontal(this.headerStyle);

        GUILayout.Label(this.title);
        GUILayout.FlexibleSpace();

        if (this.collapsed) {
            if (GUILayout.Button("Expand")) {
                this.collapsed = false;
            }
        } else {
            if (GUILayout.Button("Collapse")) {
                this.collapsed = true;
            }
        }

        GUILayout.EndHorizontal();

        if (!this.collapsed) {
            GUILayout.BeginVertical(this.bodyStyle);

            this.DrawBody();

            GUILayout.EndVertical();
        }

        GUILayout.EndVertical();
    }

    public override void ProcessEvent(EEEvent e) {}

    protected override bool EventOverlapCheck(EEEvent e) {
        return false;
    }

    public override int Layer() {
        return 110;
    }

}