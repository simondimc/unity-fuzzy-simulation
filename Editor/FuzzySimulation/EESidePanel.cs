using System;
using UnityEngine;
using System.Collections.Generic;

public class EESidePanel : EditorElement {

    private Rect rect;
    private GUIStyle style;
    private Vector2 scroll;
    private List<EESidePanelSection> sidePanelSections;

    public EESidePanel(float x, float y, float width, float height, Action<Drive> OpenDrive) {
        this.rect = new Rect(x, y, width, height);
        this.style = new GUIStyle();
        this.style.normal.background = EEUtils.ColorTexture(EETheme.AccentColor1);
        this.sidePanelSections = new List<EESidePanelSection>();
        this.sidePanelSections.Add(new EEInputVariablesSection());
        this.sidePanelSections.Add(new EEOutputVariablesSection());
        this.sidePanelSections.Add(new EEDrivesSection(OpenDrive));
    }

    public override void Draw() {
        GUI.Box(this.rect, "", this.style);

        GUILayout.BeginArea(this.rect);

        this.scroll = GUILayout.BeginScrollView(this.scroll, false, false);

        for (int i = 0; i < this.sidePanelSections.Count; i++) {
            this.sidePanelSections[i].Draw();
        }

        GUILayout.EndScrollView();

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

    public void SetHeight(float height) {
        this.rect.height = height;
    }

}