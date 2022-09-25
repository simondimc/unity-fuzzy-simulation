using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class FuzzyValues : EditorWindow {

    public static FuzzyController FuzzyController;
    public static Variable Variable;

    private GUIStyle backgroundStyle;
    private Vector2 scroll;
    private List<EEVariableValueRow> variableValueRows;
    private GUIStyle sectionStyle;
    private GUIStyle headerStyle;
    private GUIStyle bodyStyle;

    [MenuItem("Window/Fuzzy Values")]
    private static void OpenWindow() {
        FuzzyValues window = GetWindow<FuzzyValues>();
        window.titleContent = new GUIContent("Fuzzy Values");
    }

    public static void Open(FuzzyController fuzzyController, Variable variable) {
        FuzzyValues.FuzzyController = fuzzyController;
        FuzzyValues.Variable = variable;
        FuzzyValues.OpenWindow();
    }

    private void OnEnable() {
        this.backgroundStyle = new GUIStyle();
        this.backgroundStyle.normal.background = EEUtils.ColorTexture(EETheme.AccentColor1);

        this.sectionStyle = new GUIStyle();
        this.sectionStyle.margin = new RectOffset(5, 5, 5, 10);

        this.headerStyle = new GUIStyle();
        this.headerStyle.normal.background = EEUtils.ColorTexture(EETheme.HeaderColor);
        this.headerStyle.padding = new RectOffset(5, 5, 5, 5);

        this.bodyStyle = new GUIStyle();
        this.bodyStyle.normal.background = EEUtils.ColorTexture(EETheme.BodyColor);
        this.bodyStyle.padding = new RectOffset(5, 5, 5, 5);

        this.variableValueRows = new List<EEVariableValueRow>();

        this.CreateVariableValuesRows();
    }

    private void CreateVariableValuesRows() {
        this.variableValueRows.Clear();
        IEnumerable<VariableValue> variableValues = FuzzyValues.FuzzyController.GetVariableValues().Where(x => x.variableGuid == FuzzyValues.Variable.guid).ToList();

        foreach (VariableValue variableValue in variableValues) {
            this.variableValueRows.Add(new EEVariableValueRow(variableValue, OnSaveVariableValue, OnCancelVariableValue, OnEditVariableValue, OnDeleteVariableValue));
        }
    }


    private void OnGUI() {
        Draw();
        ProcessEvent(Event.current);
        if (GUI.changed) Repaint();
    }

    
    private void Draw() {
        GUI.Box(new Rect(0, 0, this.position.width, this.position.height), "", this.backgroundStyle);

        scroll = GUILayout.BeginScrollView(scroll, false, false);

        GUILayout.BeginVertical(this.sectionStyle);

        GUILayout.BeginHorizontal(this.headerStyle);

        GUILayout.Label("Variable: ");
        EditorGUILayout.SelectableLabel(FuzzyValues.Variable.name, EETheme.TextFieldStyle, GUILayout.Height(20));

        GUILayout.Label("Bounds: ");
        EditorGUILayout.SelectableLabel(FuzzyValues.Variable.lowerBound.ToString(), EETheme.TextFieldStyle, GUILayout.Height(20), GUILayout.Width(50));
        GUILayout.Label("-");
        EditorGUILayout.SelectableLabel(FuzzyValues.Variable.upperBound.ToString(), EETheme.TextFieldStyle, GUILayout.Height(20), GUILayout.Width(50));

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical(this.bodyStyle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create Value")) {
            this.OnAddVariableValue();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Name", GUILayout.Height(20));
        GUILayout.Space(5);
        GUILayout.Label("Membership Function", GUILayout.Width(140 - 10), GUILayout.Height(20));
        GUILayout.Space(5);
        GUILayout.Label("", GUILayout.Width(55 + 55 + 3 - 40), GUILayout.Height(20));
        GUILayout.EndHorizontal();

        EEUtils.DrawLine(EETheme.GreyColor);

        foreach (EEVariableValueRow variableValueRow in this.variableValueRows.ToList()) {
            GUILayout.Space(2);
            variableValueRow.Draw();
            GUILayout.Space(2);
            EEUtils.DrawLine(EETheme.GreyColor);
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.EndVertical();
    }

    private void ProcessEvent(Event e) {

    }

    private void OnAddVariableValue() {
        EEVariableValueRow newVariableValue = new EEVariableValueRow(new VariableValue(System.Guid.Empty, "", FuzzyValues.Variable.guid), OnSaveVariableValue, OnCancelVariableValue, OnEditVariableValue, OnDeleteVariableValue);
        newVariableValue.SetEditing(true);
        this.variableValueRows.Add(newVariableValue);
        foreach (EEVariableValueRow variableValueRow in variableValueRows) {
            if (variableValueRow.GetId() != newVariableValue.GetId()) {
                variableValueRow.SetEditing(false);
            }
        }
    }

    private void OnSaveVariableValue(VariableValue variableValue) {
        if (variableValue.guid == System.Guid.Empty.ToString()) {
            variableValue = variableValue.CopyNewGuid();
            FuzzyValues.FuzzyController.AddVariableValue(variableValue);
        } else {
            FuzzyValues.FuzzyController.SetVariableValue(variableValue.guid, variableValue);
        }
        EEUtils.FixVariableValue(variableValue);
        this.CreateVariableValuesRows();
    }

    private void OnCancelVariableValue(VariableValue variableValue) {
        this.CreateVariableValuesRows();
    }

    private void OnEditVariableValue(VariableValue variableValue) {
        foreach (EEVariableValueRow variableValueRow in variableValueRows) {
            if (variableValueRow.GetId() != variableValue.guid) {
                variableValueRow.SetEditing(false);
            }
        }
    }

    private void OnDeleteVariableValue(VariableValue variableValue) {
        FuzzyValues.FuzzyController.RemoveVariableValue(variableValue);
        this.CreateVariableValuesRows();
    }

}