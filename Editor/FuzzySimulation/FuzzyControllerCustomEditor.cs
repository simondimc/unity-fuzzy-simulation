using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class AssetHandler {
    
    [OnOpenAsset()]
    public static bool OpenEditor(int instanceId, int line) {
        FuzzyController fuzzyController = EditorUtility.InstanceIDToObject(instanceId) as FuzzyController;
        if (fuzzyController != null) {
            FuzzyRules.Open((FuzzyController)fuzzyController);
            return true;
        }
        return false;
    }
    
}

[CustomEditor(typeof(FuzzyController))]
public class FuzzyControllerCustomEditor : Editor {

    public override void OnInspectorGUI() {
        if (GUILayout.Button("Open Editor")) {
            FuzzyRules.Open((FuzzyController)target);
        }
    }

}