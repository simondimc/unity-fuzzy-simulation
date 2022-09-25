using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour {

    [Header("Fuzzy Agent")]
    public float PerceptionRadius = 4;
    public float HorizontalFOV = 135;
    public float VerticalFOV = 180;

    public List<Agent> Neighbors { get; set; }
    public abstract Vector3 Position { get; set; }
    public abstract Vector3 Direction { get; set; }
    public abstract float Speed { get; set; }

    private FuzzyController fuzzyController;

    public FuzzyController GetFuzzyController() {
        return this.fuzzyController;
    }

    public void SetFuzzyController(FuzzyController fuzzyController) {
        this.fuzzyController = fuzzyController;
    }

    public override bool Equals(object other) {
        if ((other == null) || !this.GetType().Equals(other.GetType())) {
            return false;
        }
        Agent otherAgent = (Agent) other;
        return this.gameObject.Equals(otherAgent.gameObject);
    }

    public override int GetHashCode() {
        return this.gameObject.GetHashCode();
    }

}
