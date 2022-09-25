using System.Collections.Generic;

public class EEInputVariablesSection : EEVariablesSection {

    public EEInputVariablesSection(): 
    base("Input Variables") {}

    public override IEnumerable<Variable> GetVariables() {
        return FuzzyRules.FuzzyController.GetInputVariables();
    }

    public override void AddVariable(Variable variable) {
        FuzzyRules.FuzzyController.AddInputVariable(variable);
    }

    public override void SetVariable(string guid, Variable variable) {
        FuzzyRules.FuzzyController.SetInputVariable(guid, variable);
    }

    public override void RemoveVariable(Variable variable) {
        FuzzyRules.FuzzyController.RemoveInputVariable(variable);
    }

}