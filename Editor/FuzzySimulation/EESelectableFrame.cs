using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class EESelectableFrame : EEFrame {

    private bool isSelected;
    private GUIStyle selectedStyle;
    private GUIStyle borderStyle;
    private Rect borderRect;
    private List<GenericMenuItem> genericMenuItems;

    public EESelectableFrame(Rect rect): 
    base(rect) {
        this.borderRect = new Rect(this.GetFrameRect().x - 1.5f, this.GetFrameRect().y - 1.5f, this.GetFrameRect().width + 3, this.GetFrameRect().height + 3);

        this.borderStyle = new GUIStyle();

        this.borderStyle.normal.background = EEUtils.ColorTexture(EETheme.AccentColor1);

        this.selectedStyle = new GUIStyle();

        this.selectedStyle.normal.background =  EEUtils.ColorTexture(EETheme.AccentColor3);

        this.genericMenuItems = new List<GenericMenuItem>();
    }

    public override void Draw() {
        if (this.isSelected) {
            GUI.Box(this.borderRect, "", this.selectedStyle);
        } else {
            GUI.Box(this.borderRect, "", this.borderStyle);
        }

        base.Draw();
    }

    public override void ProcessEvent(EEEvent e) {
        base.ProcessEvent(e);

        switch (e.GetEvent().type) {
            case EventType.MouseDown:
                if (e.GetEvent().button == 0 || e.GetEvent().button == 1) {
                    if (this.GetFrameRect().Contains(e.GetEvent().mousePosition) && !e.Used()) {
                        this.isSelected = true;
                        GUI.changed = true;
                    } else {
                        this.isSelected = false;
                        GUI.changed = true;
                    }
                }
                if (e.GetEvent().button == 1 && this.isSelected && this.GetFrameRect().Contains(e.GetEvent().mousePosition)) {
                    this.ProcessContextMenu();
                    e.RealUse();
                }
                break;
        }
    }

    private void ProcessContextMenu() {
        GenericMenu genericMenu = new GenericMenu();
        for (int i = 0; i < this.genericMenuItems.Count; i++) {
            genericMenu.AddItem(this.genericMenuItems[i].GetContent(), false, this.genericMenuItems[i].GetFunc());
        }
        genericMenu.ShowAsContext();
    }

    public override void Move(Vector2 delta) {
        base.Move(delta);
        this.borderRect.position += delta;
    }

    public Rect GetBorderRect() {
        return this.borderRect;
    }

    public void AddGenericMenuItem(GenericMenuItem genericMenuItem) {
        this.genericMenuItems.Add(genericMenuItem);
    }

    public override void SetSize(Vector2 size) {
        base.SetSize(size);
        this.borderRect.size = this.GetFrameRect().size + new Vector2(3, 3);
    }

}