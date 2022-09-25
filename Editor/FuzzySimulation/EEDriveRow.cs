using System;
using UnityEditor;
using UnityEngine;

public class EEDriveRow : EditorElement {

    private Drive drive;
    private bool isEditing;
    private Drive editableDrive;
    private Action<Drive> OnSaveDrive;
    private Action<Drive> OnCancelDrive;
    private Action<Drive> OnEditDrive;
    private Action<Drive> OnDeleteDrive;
    private Action<Drive> OnOpenDrive;
    private bool focusNameTextArea;

    public string GetId() {
        return this.drive.guid;
    }

    public EEDriveRow(Drive drive, Action<Drive> OnSaveDrive, Action<Drive> OnCancelDrive, Action<Drive> OnEditDrive, Action<Drive> OnDeleteDrive, Action<Drive> OnOpenDrive) {
        this.drive = drive;
        this.editableDrive = drive.Copy();
        this.isEditing = false;
        this.OnSaveDrive = OnSaveDrive;
        this.OnCancelDrive = OnCancelDrive;
        this.OnEditDrive = OnEditDrive;
        this.OnDeleteDrive = OnDeleteDrive;
        this.OnOpenDrive = OnOpenDrive;
        this.focusNameTextArea = false;
    }

    public override void Draw() {
        GUILayout.BeginHorizontal();
        if (this.isEditing) {
            GUI.SetNextControlName("nameTextArea");
            this.editableDrive.name = EditorGUILayout.TextField(this.editableDrive.name, GUILayout.Height(20));
            if (this.focusNameTextArea) {
                GUI.FocusControl("nameTextArea");
                this.focusNameTextArea = false;
            }
        } else {
            EditorGUILayout.SelectableLabel(this.drive.name, EETheme.TextFieldStyle, GUILayout.Height(20));
        }
        GUILayout.Space(5);
        if (this.isEditing) {
            if (GUILayout.Button("Save", GUILayout.Width(55), GUILayout.Height(20))) {
                this.drive = this.editableDrive.Copy();
                this.SetEditing(false);

                if (!string.IsNullOrWhiteSpace(this.drive.name)) {
                    this.OnSaveDrive(this.drive);
                } else {
                    this.OnCancelDrive(this.drive);
                }
            }
            if (GUILayout.Button("Cancel", GUILayout.Width(55), GUILayout.Height(20))) {
                this.SetEditing(false);
                this.OnCancelDrive(this.drive);
            }
            GUILayout.Space(58);
        } else {
            if (!this.drive.isOpen) {
                if (GUILayout.Button("Open", GUILayout.Width(55), GUILayout.Height(20))) {
                    this.SetIsOpen(true);
                    this.OnOpenDrive(this.drive);
                }
            } else {
                GUILayout.Space(58);
            }
            if (GUILayout.Button("Edit", GUILayout.Width(55), GUILayout.Height(20))) {
                this.SetEditing(true);
                this.OnEditDrive(this.drive);
            }
            if (GUILayout.Button("Del", GUILayout.Width(55), GUILayout.Height(20))) {
                this.OnDeleteDrive(this.drive);
            }
        }
        
        GUILayout.EndHorizontal();
    }

    public void SetIsOpen(bool isOpen) {
        this.drive.isOpen = isOpen;
        FuzzyRules.FuzzyController.SetDrive(this.drive.guid, this.drive);
    }

    public void SetEditing(bool isEditing) {
        this.isEditing = isEditing;
        if (isEditing) {
            this.editableDrive = this.drive.Copy();
            this.focusNameTextArea = true;
        } else {
            GUI.FocusControl(null);
        }
    }

    public override void ProcessEvent(EEEvent e) {}

    protected override bool EventOverlapCheck(EEEvent e) {
        return false;
    }

    public override int Layer() {
        return 0;
    }

}