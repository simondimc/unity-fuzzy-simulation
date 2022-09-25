using System;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class EEVariableValueRow : EditorElement {

    private VariableValue variableValue;
    private bool isEditing;
    private VariableValue editableVariableValue;
    private Action<VariableValue> OnSaveVariableValue;
    private Action<VariableValue> OnCancelVariableValue;
    private Action<VariableValue> OnEditVariableValue;
    private Action<VariableValue> OnDeleteVariableValue;
    private bool focusNameTextArea;
    private AnimationCurve curve;

    public string GetId() {
        return this.variableValue.guid;
    }

    public EEVariableValueRow(VariableValue variableValue, Action<VariableValue> OnSaveVariableValue, Action<VariableValue> OnCancelVariableValue, Action<VariableValue> OnEditVariableValue, Action<VariableValue> OnDeleteVariableValue) {
        this.variableValue = variableValue;
        this.editableVariableValue = variableValue.Copy();
        this.isEditing = false;
        this.OnSaveVariableValue = OnSaveVariableValue;
        this.OnCancelVariableValue = OnCancelVariableValue;
        this.OnEditVariableValue = OnEditVariableValue;
        this.OnDeleteVariableValue = OnDeleteVariableValue;
        this.focusNameTextArea = false;
        this.curve = new AnimationCurve();
        if (this.variableValue != null) {
            EEUtils.SetKeyframes(this.curve, variableValue.graphPoints);
        }
    }

    public override void Draw() {

        this.curve = new AnimationCurve();
        if (this.variableValue != null) {
            EEUtils.SetKeyframes(this.curve, this.variableValue.graphPoints);
        }

        GUILayout.BeginHorizontal();
        if (this.isEditing) {
            GUI.SetNextControlName("nameTextArea");
            this.editableVariableValue.name = EditorGUILayout.TextField(this.editableVariableValue.name, GUILayout.Height(20));
            if (this.focusNameTextArea) {
                GUI.FocusControl("nameTextArea");
                this.focusNameTextArea = false;
            }
        } else {
            EditorGUILayout.SelectableLabel(this.variableValue.name, EETheme.TextFieldStyle, GUILayout.Height(20));
        }
        GUILayout.Space(5);

        Rect ranges = new Rect(0, 0, 1, 1);
        Variable variable = FuzzyRules.FuzzyController.GetVariables().Where(x => x.guid == FuzzyValues.Variable.guid).First();
        ranges.x = variable.lowerBound;
        ranges.width = variable.upperBound - variable.lowerBound;

        if (this.isEditing) GUI.enabled = false;
        EditorGUI.BeginChangeCheck();
        this.curve = EditorGUILayout.CurveField(this.curve, Color.white, ranges, GUILayout.Height(33), GUILayout.Width(100 - 10));
        if (EditorGUI.EndChangeCheck()) {
            bool hasLowerBound = false;
            bool hasUpperBound = false;
            foreach (Keyframe keyframe in this.curve.keys) {
                if (keyframe.time == variable.lowerBound) {
                    hasLowerBound = true;
                }
                if (keyframe.time == variable.upperBound) {
                    hasUpperBound = true;
                }
            }
            if (!hasLowerBound) {
                int i = this.curve.AddKey(variable.lowerBound, 0);
                AnimationUtility.SetKeyLeftTangentMode(this.curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(this.curve, i, AnimationUtility.TangentMode.Linear);
            }
            if (!hasUpperBound) {
                int i = this.curve.AddKey(variable.upperBound, 0);
                AnimationUtility.SetKeyLeftTangentMode(this.curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(this.curve, i, AnimationUtility.TangentMode.Linear);
            }
 
            if (this.isEditing) {
                this.editableVariableValue.graphPoints = EEUtils.GetGraphPoints(this.curve);
                this.editableVariableValue.graphSamples = EEUtils.GetGraphSamples(variable, this.curve);
            } else {
                this.variableValue.graphPoints = EEUtils.GetGraphPoints(this.curve);
                this.variableValue.graphSamples = EEUtils.GetGraphSamples(variable, this.curve);

                FuzzyRules.FuzzyController.SetVariableValue(this.variableValue.guid, this.variableValue);
            }
        }
        if (this.isEditing) GUI.enabled = true;

        GUILayout.Space(5);
        if (this.isEditing) {
            if (GUILayout.Button("Save", GUILayout.Width(55), GUILayout.Height(20))) {
                this.variableValue = this.editableVariableValue.Copy();
                this.isEditing = false;
                GUI.FocusControl(null);
                this.OnSaveVariableValue(this.variableValue);
            }
            if (GUILayout.Button("Cancel", GUILayout.Width(55), GUILayout.Height(20))) {
                this.SetEditing(false);
                this.OnCancelVariableValue(this.variableValue);
            }
        } else {
            if (GUILayout.Button("Edit", GUILayout.Width(55), GUILayout.Height(20))) {
                this.SetEditing(true);
                this.OnEditVariableValue(this.variableValue);
            }
            if (GUILayout.Button("Del", GUILayout.Width(55), GUILayout.Height(20))) {
                this.OnDeleteVariableValue(this.variableValue);
            }
        }
        
        GUILayout.EndHorizontal();
    }

    public void SetEditing(bool isEditing) {
        this.isEditing = isEditing;
        if (isEditing) {
            this.editableVariableValue = this.variableValue.Copy();
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