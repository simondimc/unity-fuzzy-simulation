using System;
using UnityEditor;
using UnityEngine;

public class EEVariableRow : EditorElement {

    private Variable variable;
    private bool isEditing;
    private Variable editableVariable;
    private Action<Variable> OnSaveVariable;
    private Action<Variable> OnCancelVariable;
    private Action<Variable> OnEditVariable;
    private Action<Variable> OnDeleteVariable;
    private bool focusNameTextArea;

    public string GetId() {
        return this.variable.guid;
    }

    public EEVariableRow(Variable variable, Action<Variable> OnSaveVariable, Action<Variable> OnCancelVariable, Action<Variable> OnEditVariable, Action<Variable> OnDeleteVariable) {
        this.variable = variable;
        this.editableVariable = variable.Copy();
        this.isEditing = false;
        this.OnSaveVariable = OnSaveVariable;
        this.OnCancelVariable = OnCancelVariable;
        this.OnEditVariable = OnEditVariable;
        this.OnDeleteVariable = OnDeleteVariable;
        this.focusNameTextArea = false;
    }

    public override void Draw() {
        GUILayout.BeginHorizontal();
        if (this.isEditing) {
            GUI.SetNextControlName("nameTextArea");
            this.editableVariable.name = EditorGUILayout.TextField(this.editableVariable.name, GUILayout.Height(20));
            if (this.focusNameTextArea) {
                GUI.FocusControl("nameTextArea");
                this.focusNameTextArea = false;
            }
        } else {
            EditorGUILayout.SelectableLabel(this.variable.name, EETheme.TextFieldStyle, GUILayout.Height(20));
        }
        GUILayout.Space(5);
        if (this.isEditing) {
            this.editableVariable.lowerBound = EditorGUILayout.FloatField(this.editableVariable.lowerBound, GUILayout.Width(35), GUILayout.Height(20));
        } else {
            EditorGUILayout.SelectableLabel(this.variable.lowerBound.ToString(), EETheme.TextFieldStyle, GUILayout.Width(35), GUILayout.Height(20));
        }
        GUILayout.Space(5);
        if (this.isEditing) {
            this.editableVariable.upperBound = EditorGUILayout.FloatField(this.editableVariable.upperBound, GUILayout.Width(35), GUILayout.Height(20));
        } else {
            EditorGUILayout.SelectableLabel(this.variable.upperBound.ToString(), EETheme.TextFieldStyle, GUILayout.Width(35), GUILayout.Height(20));
        }
        GUILayout.Space(5);
        if (this.isEditing) {
            if (GUILayout.Button("Save", GUILayout.Width(55), GUILayout.Height(20))) {
                this.variable = this.editableVariable.Copy();
                this.SetEditing(false);

                if (!string.IsNullOrWhiteSpace(this.variable.name) && this.variable.lowerBound != this.variable.upperBound && this.variable.lowerBound < this.variable.upperBound) {
                    this.OnSaveVariable(this.variable);
                } else {
                    this.OnCancelVariable(this.variable);
                }
            }
            if (GUILayout.Button("Cancel", GUILayout.Width(55), GUILayout.Height(20))) {
                this.SetEditing(false);
                this.OnCancelVariable(this.variable);
            }
            GUILayout.Space(55 + 3);
        } else {
            if (GUILayout.Button("Edit", GUILayout.Width(55), GUILayout.Height(20))) {
                this.SetEditing(true);
                this.OnEditVariable(this.variable);
            }
            if (GUILayout.Button("Del", GUILayout.Width(55), GUILayout.Height(20))) {
                this.OnDeleteVariable(this.variable);
            }
            if (GUILayout.Button("Values", GUILayout.Width(55), GUILayout.Height(20))) {
                FuzzyValues.Open(FuzzyRules.FuzzyController, this.variable);
            }
        }
        
        GUILayout.EndHorizontal();
    }

    public void SetEditing(bool isEditing) {
        this.isEditing = isEditing;
        if (isEditing) {
            this.editableVariable = this.variable.Copy();
            this.focusNameTextArea = true;
        } else {
            GUI.FocusControl(null);
        }
    }

    public override int Layer() {
        return 0;
    }

    public override void ProcessEvent(EEEvent e) {}

    protected override bool EventOverlapCheck(EEEvent e) {
        return false;
    }

}