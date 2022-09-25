using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EEDrivesSection : EESidePanelSection {
    
    private List<EEDriveRow> driveRows;
    private Action<Drive> OpenDrive;

    public EEDrivesSection(Action<Drive> OpenDrive): 
    base("Drives") {
        this.driveRows = new List<EEDriveRow>();
        this.OpenDrive = OpenDrive;
        this.CreateDriveRows();
    }

    private void CreateDriveRows() {
        this.driveRows.Clear();
        IEnumerable<Drive> drives = FuzzyRules.FuzzyController.GetDrives();

        foreach (Drive drive in drives) {
            this.driveRows.Add(new EEDriveRow(drive, this.OnSaveDrive, this.OnCancelDrive, this.OnEditDrive, this.OnDeleteDrive, this.OnOpenDrive));
        }
    }

    public override void DrawBody() {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create Drive")) {
            this.OnAddDrive();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Name", GUILayout.Height(20));
        GUILayout.Space(5);
        GUILayout.Label("", GUILayout.Width(110 + 15), GUILayout.Height(20));
        GUILayout.EndHorizontal();

        EEUtils.DrawLine(EETheme.GreyColor);
        foreach (EEDriveRow driveRow in this.driveRows.ToList()) {
            GUILayout.Space(2);
            driveRow.Draw();
            GUILayout.Space(2);
            EEUtils.DrawLine(EETheme.GreyColor);
        }
    }

    private void OnAddDrive() {
        EEDriveRow newDrive = new EEDriveRow(new Drive(System.Guid.Empty, ""), this.OnSaveDrive, this.OnCancelDrive, this.OnEditDrive, this.OnDeleteDrive, this.OnOpenDrive);
        newDrive.SetEditing(true);
        this.driveRows.Add(newDrive);
        foreach (EEDriveRow driveRow in this.driveRows) {
            if (driveRow.GetId() != newDrive.GetId()) {
                driveRow.SetEditing(false);
            }
        }
    }

    private void OnSaveDrive(Drive drive) {
        if (drive.guid == System.Guid.Empty.ToString()) {
            FuzzyRules.FuzzyController.AddDrive(drive.CopyNewGuid());
        } else {
            FuzzyRules.FuzzyController.SetDrive(drive.guid, drive);
        }
        this.CreateDriveRows();
    }

    private void OnCancelDrive(Drive drive) {
        this.CreateDriveRows();
    }

    private void OnEditDrive(Drive drive) {
        foreach (EEDriveRow driveRow in this.driveRows) {
            if (driveRow.GetId() != drive.guid) {
                driveRow.SetEditing(false);
            }
        }
    }

    private void OnDeleteDrive(Drive drive) {
        FuzzyRules.FuzzyController.RemoveDrive(drive);
        this.CreateDriveRows();
        IEnumerable<Drive> drives = FuzzyRules.FuzzyController.GetDrives();
        this.OnOpenDrive(drives.First());
    }

    private void OnOpenDrive(Drive drive) {
        this.OpenDrive(drive);
        foreach (EEDriveRow driveRow in this.driveRows) {
            if (driveRow.GetId() != drive.guid) {
                driveRow.SetIsOpen(false);
            }
        }
    }

}