using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public abstract class EEVariablesSection : EESidePanelSection {

    private List<EEVariableRow> variableRows;

    public abstract IEnumerable<Variable> GetVariables();
    public abstract void AddVariable(Variable variable);
    public abstract void SetVariable(string guid, Variable variable);
    public abstract void RemoveVariable(Variable variable);

    public EEVariablesSection(string title): 
    base(title) {
        this.variableRows = new List<EEVariableRow>();
        this.CreateVariableRows();
    }

    private void CreateVariableRows() {
        this.variableRows.Clear();
        IEnumerable<Variable> variables = this.GetVariables();

        foreach (Variable variable in variables) {
            this.variableRows.Add(new EEVariableRow(variable, this.OnSaveVariable, this.OnCancelVariable, this.OnEditVariable, this.OnDeleteVariable));
        }
    }

    public override void DrawBody() {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create Variable")) {
            this.OnAddVariable();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Name", GUILayout.Height(20));
        GUILayout.Space(5);
        GUILayout.Label("Min", GUILayout.Width(35), GUILayout.Height(20));
        GUILayout.Space(5);
        GUILayout.Label("Max", GUILayout.Width(35), GUILayout.Height(20));
        GUILayout.Space(5);
        GUILayout.Label("", GUILayout.Width(55 + 55 + 55 + 5), GUILayout.Height(20));
        GUILayout.EndHorizontal();

        EEUtils.DrawLine(EETheme.GreyColor);
        foreach (EEVariableRow variableRow in this.variableRows.ToList()) {
            GUILayout.Space(2);
            variableRow.Draw();
            GUILayout.Space(2);
            EEUtils.DrawLine(EETheme.GreyColor);
        }
    }

    private void OnAddVariable() {
        EEVariableRow newVariable = new EEVariableRow(new Variable(System.Guid.Empty, "", 0, 1), this.OnSaveVariable, this.OnCancelVariable, this.OnEditVariable, this.OnDeleteVariable);
        newVariable.SetEditing(true);
        this.variableRows.Add(newVariable);
        foreach (EEVariableRow variableRow in this.variableRows) {
            if (variableRow.GetId() != newVariable.GetId()) {
                variableRow.SetEditing(false);
            }
        }
    }

    private void OnSaveVariable(Variable variable) {
        if (variable.guid == System.Guid.Empty.ToString()) {
            variable = variable.CopyNewGuid();
            this.AddVariable(variable);
        } else {
            this.SetVariable(variable.guid, variable);
        }
        EEUtils.FixVariableValues(variable);
        this.CreateVariableRows();
    }

    private void OnCancelVariable(Variable variable) {
        this.CreateVariableRows();
    }

    private void OnEditVariable(Variable variable) {
        foreach (EEVariableRow variableRow in this.variableRows) {
            if (variableRow.GetId() != variable.guid) {
                variableRow.SetEditing(false);
            }
        }
    }

    private void OnDeleteVariable(Variable variable) {
        this.RemoveVariable(variable);
        this.CreateVariableRows();
    }

}