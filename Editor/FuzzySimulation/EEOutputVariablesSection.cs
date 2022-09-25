using System.Collections.Generic;

public class EEOutputVariablesSection : EEVariablesSection {
    
    public EEOutputVariablesSection(): 
    base("Output Variables") {}

    public override IEnumerable<Variable> GetVariables() {
        return FuzzyRules.FuzzyController.GetOutputVariables();
    }

    public override void AddVariable(Variable variable) {
        FuzzyRules.FuzzyController.AddOutputVariable(variable);
    }

    public override void SetVariable(string guid, Variable variable) {
        FuzzyRules.FuzzyController.SetOutputVariable(guid, variable);
    }

    public override void RemoveVariable(Variable variable) {
        FuzzyRules.FuzzyController.RemoveOutputVariable(variable);
    }
}