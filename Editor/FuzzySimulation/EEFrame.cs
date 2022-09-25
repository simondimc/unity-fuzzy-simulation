using UnityEngine;

public class EEFrame : EditorElement {

    private Rect frameRect;
    private Rect rect;
    private GUIStyle rectStyle;
    private GUIStyle headerStyle;
    private GUIStyle titleStyle;
    private string title;

    public EEFrame(Rect rect) {
        this.title = "";
        this.frameRect = new Rect(rect.x, rect.y - 18, rect.width, rect.height + 18);
        this.rect = rect;

        this.rectStyle = new GUIStyle();
        this.rectStyle.normal.background = EEUtils.ColorTexture(EETheme.BodyColor);

        this.headerStyle = new GUIStyle();
        this.headerStyle.normal.background = EEUtils.ColorTexture(EETheme.HeaderColor);

        this.titleStyle = new GUIStyle();
        this.titleStyle.fontSize = 12;
        this.titleStyle.alignment = TextAnchor.MiddleLeft;
        this.titleStyle.normal.textColor = EETheme.LightColor;
    }

    public override void Draw() {
        GUI.Box(this.frameRect, "", this.headerStyle);

        GUILayout.BeginArea(this.frameRect);

        GUILayout.BeginHorizontal();

        GUILayout.Space(5);

        GUILayout.Label(this.title, this.titleStyle);

        GUILayout.Space(5);

        GUILayout.EndHorizontal();

        GUILayout.EndArea();

        GUI.Box(this.rect, "", this.rectStyle);
    }

    public override int Layer() {
        return 0;
    }

    public override void ProcessEvent(EEEvent e) {}

    protected override bool EventOverlapCheck(EEEvent e) {
        switch (e.GetEvent().type) {
            case EventType.MouseDown:
                if (e.GetEvent().button == 0 || e.GetEvent().button == 1) {
                    if (this.frameRect.Contains(e.GetEvent().mousePosition)) {
                        return true;
                    }
                }
                break;
        }
        return false;
    }

    public virtual void Move(Vector2 delta) {
        this.frameRect.position += delta;
        this.rect.position += delta;
    }

    public Rect GetRect() {
        return this.rect;
    }

    public Rect GetFrameRect() {
        return this.frameRect;
    }

    public virtual void SetSize(Vector2 size) {
        this.rect.size = size;
        this.frameRect.size = size + new Vector2(0, 18);
    }

    public string GetTitle() {
        return this.title;
    }

    public void SetTitle(string title) {
        this.title = title;
    }

}