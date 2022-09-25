using UnityEditor;
using UnityEngine;

public class EEDriveInfo : EditorElement {

    private Rect rect;
    private GUIStyle style;
    private Drive drive;
    private GUIStyle areaStyle;

    public EEDriveInfo(float x, float y, float width, float height) {
        this.rect = new Rect(x, y, width, height);
        this.style = new GUIStyle();
        this.style.normal.background = EEUtils.ColorTexture(EETheme.AccentColor1);
        this.drive = null;
        this.areaStyle = new GUIStyle();
        this.areaStyle.padding = new RectOffset(5, 5, 5, 5);
    }

    public override void Draw() {
        GUI.Box(this.rect, "", this.style);

        GUILayout.BeginArea(this.rect, this.areaStyle);

        GUILayout.BeginHorizontal();

        GUILayout.Label("Drive:", GUILayout.Width(45));

        if (this.drive != null) {
            EditorGUILayout.SelectableLabel(this.drive.name, EETheme.TextFieldStyle, GUILayout.Height(20));
        }

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

    public void SetDrive(Drive drive) {
        this.drive = drive;
    }

}