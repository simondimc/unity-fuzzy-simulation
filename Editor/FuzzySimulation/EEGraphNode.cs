using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum EEGraphNodeType {
    Input, 
    Output
}

public class EEGraphNode : EENode {

    private int variableIndex;
    private int variableValueIndex;
    private AnimationCurve curve;
    private Action<EENode> OnAddConnection;
    private Action<EENode> OnDelete;
    private EEGraphNodeType graphNodeType;

    private bool prevDriveMinView;

    public EEGraphNode(GraphNode graphNode, EEGraphNodeType graphNodeType, Action<EENode> OnAddConnection, Action<EENode> OnDelete, Action<EENode> OnNodeClicked): 
    base(new Rect(graphNode.x, graphNode.y, 100, 124), graphNode, OnNodeClicked) {
        this.graphNodeType = graphNodeType;
        if (this.graphNodeType == EEGraphNodeType.Input) {
            this.SetTitle("Input Variable");
        } else if (this.graphNodeType == EEGraphNodeType.Output) {
            this.SetTitle("Output Variable");
        }
        this.variableIndex = -1;
        this.variableValueIndex = -1;
        if (this.graphNodeType == EEGraphNodeType.Input) {
            this.variableIndex = FuzzyRules.FuzzyController.GetInputVariables().ToList().FindIndex(x => x.guid == ((GraphNode)this.GetNode()).variableGuid);
        } else if (this.graphNodeType == EEGraphNodeType.Output) {
            this.variableIndex = FuzzyRules.FuzzyController.GetOutputVariables().ToList().FindIndex(x => x.guid == ((GraphNode)this.GetNode()).variableGuid);
        }
        if (variableIndex >= 0) {
            if (this.graphNodeType == EEGraphNodeType.Input) {
                Variable variable = FuzzyRules.FuzzyController.GetInputVariables().ToList()[variableIndex];
                this.variableValueIndex = FuzzyRules.FuzzyController.GetInputVariableValues().Where(x => x.variableGuid == variable.guid).ToList().FindIndex(x => x.guid == ((GraphNode)this.GetNode()).variableValueGuid);
            } else if (this.graphNodeType == EEGraphNodeType.Output) {
                Variable variable = FuzzyRules.FuzzyController.GetOutputVariables().ToList()[variableIndex];
                this.variableValueIndex = FuzzyRules.FuzzyController.GetOutputVariableValues().Where(x => x.variableGuid == variable.guid).ToList().FindIndex(x => x.guid == ((GraphNode)this.GetNode()).variableValueGuid);
            }
        }
        this.OnAddConnection = OnAddConnection;
        this.OnDelete = OnDelete;
        this.curve = new AnimationCurve();
        VariableValue variableValue = FuzzyRules.FuzzyController.GetVariableValues().ToList().Find(x => x.guid == ((GraphNode)this.GetNode()).variableValueGuid);
        if (variableValue != null) {
            EEUtils.SetKeyframes(this.curve, variableValue.graphPoints);
        }
        this.AddGenericMenuItem(new GenericMenuItem(new GUIContent("Make Transition"), () => this.OnAddConnection(this)));
        this.AddGenericMenuItem(new GenericMenuItem(new GUIContent("Delete"), () => this.OnDelete(this)));

        this.prevDriveMinView = FuzzyRules.Drive.isMinView;
        if (FuzzyRules.Drive.isMinView) {
            this.SetSize(new Vector2(100, 124 - 75));
        } else {
            this.SetSize(new Vector2(100, 124));
        }
    }

    public EEGraphNode(Vector2 pos, EEGraphNodeType graphNodeType, Action<EENode> OnAddConnection, Action<EENode> OnDelete, Action<EENode> OnNodeClicked):
    this(new GraphNode(pos.x, pos.y), graphNodeType, OnAddConnection, OnDelete, OnNodeClicked) {
        if (this.graphNodeType == EEGraphNodeType.Input) {
            FuzzyRules.FuzzyController.AddInputGraphNode(FuzzyRules.Drive, (GraphNode)this.GetNode());
        } else if (this.graphNodeType == EEGraphNodeType.Output) {
            FuzzyRules.FuzzyController.AddOutputGraphNode(FuzzyRules.Drive, (GraphNode)this.GetNode());
        }
    }

    public override void Move(Vector2 delta) {
        base.Move(delta);
        if (this.graphNodeType == EEGraphNodeType.Input) {
            FuzzyRules.FuzzyController.SetInputGraphNode(FuzzyRules.Drive, this.GetNode().guid, (GraphNode)this.GetNode());
        } else if (this.graphNodeType == EEGraphNodeType.Output) {
            FuzzyRules.FuzzyController.SetOutputGraphNode(FuzzyRules.Drive, this.GetNode().guid, (GraphNode)this.GetNode());
        }
    }

    public override void Draw() {

        if (FuzzyRules.Drive.isMinView != this.prevDriveMinView) {
            if (FuzzyRules.Drive.isMinView) {
                this.SetSize(new Vector2(100, 124 - 75));
                this.Move(new Vector2(0, 75 / 2));
            } else {
                this.SetSize(new Vector2(100, 124));
                this.Move(new Vector2(0, -75 / 2));
            }
            this.prevDriveMinView = FuzzyRules.Drive.isMinView;
        }

        base.Draw();

        this.curve = new AnimationCurve();
        VariableValue currVariableValue = FuzzyRules.FuzzyController.GetVariableValues().ToList().Find(x => x.guid == ((GraphNode)this.GetNode()).variableValueGuid);
        if (currVariableValue != null) {
            EEUtils.SetKeyframes(this.curve, currVariableValue.graphPoints);
        }

        GUILayout.BeginArea(this.GetRect());

        GUILayout.Space(3);

        GUILayout.BeginHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginVertical();

        if (!FuzzyRules.Drive.isMinView) {
            GUILayout.Label("Variable");
        }

        List<string> variableOptions = new List<string>();
        if (this.graphNodeType == EEGraphNodeType.Input) {
            foreach (Variable variable in FuzzyRules.FuzzyController.GetInputVariables()) {
                variableOptions.Add(variable.name);
            }
        } else if (this.graphNodeType == EEGraphNodeType.Output) {
            foreach (Variable variable in FuzzyRules.FuzzyController.GetOutputVariables()) {
                variableOptions.Add(variable.name);
            }
        }

        EditorGUI.BeginChangeCheck();
        variableIndex = EditorGUILayout.Popup(variableIndex, variableOptions.ToArray(), GUILayout.Height(20));
        if (EditorGUI.EndChangeCheck()) {
            if (this.graphNodeType == EEGraphNodeType.Input) {

                int prevVariableIndex = FuzzyRules.FuzzyController.GetInputVariables().ToList().FindIndex(x => x.guid == ((GraphNode)this.GetNode()).variableGuid);
                if (prevVariableIndex != variableIndex) {
                    this.variableValueIndex = -1;
                    ((GraphNode)this.GetNode()).variableValueGuid = null;
                }

                Variable variable = FuzzyRules.FuzzyController.GetInputVariables().ToList()[variableIndex];
                ((GraphNode)this.GetNode()).variableGuid = variable.guid;

                FuzzyRules.FuzzyController.SetInputGraphNode(FuzzyRules.Drive, GetNode().guid, (GraphNode)GetNode());
            } else if (this.graphNodeType == EEGraphNodeType.Output) {

                int prevVariableIndex = FuzzyRules.FuzzyController.GetOutputVariables().ToList().FindIndex(x => x.guid == ((GraphNode)this.GetNode()).variableGuid);
                if (prevVariableIndex != variableIndex) {
                    this.variableValueIndex = -1;
                    ((GraphNode)this.GetNode()).variableValueGuid = null;
                }

                Variable variable = FuzzyRules.FuzzyController.GetOutputVariables().ToList()[variableIndex];
                ((GraphNode)this.GetNode()).variableGuid = variable.guid;

                FuzzyRules.FuzzyController.SetOutputGraphNode(FuzzyRules.Drive, GetNode().guid, (GraphNode)GetNode());
            }
        }

        if (!FuzzyRules.Drive.isMinView) {
            GUILayout.Label("Value");
        }

        List<string> variableValueOptions = new List<string>();
        if (this.variableIndex >= 0) {
            if (this.graphNodeType == EEGraphNodeType.Input) {
                Variable variable = FuzzyRules.FuzzyController.GetInputVariables().ToList()[variableIndex];
                foreach (VariableValue variableValue in FuzzyRules.FuzzyController.GetInputVariableValues().Where(x => x.variableGuid == variable.guid)) {
                    variableValueOptions.Add(variableValue.name);
                }
            } else if (this.graphNodeType == EEGraphNodeType.Output) {
                Variable variable = FuzzyRules.FuzzyController.GetOutputVariables().ToList()[variableIndex];
                foreach (VariableValue variableValue in FuzzyRules.FuzzyController.GetOutputVariableValues().Where(x => x.variableGuid == variable.guid)) {
                    variableValueOptions.Add(variableValue.name);
                }
            }
        }

        EditorGUI.BeginChangeCheck();
        variableValueIndex = EditorGUILayout.Popup(variableValueIndex, variableValueOptions.ToArray(), GUILayout.Height(20));
        if (EditorGUI.EndChangeCheck()) {
            if (this.graphNodeType == EEGraphNodeType.Input) {
                Variable variable = FuzzyRules.FuzzyController.GetInputVariables().ToList()[variableIndex];
                VariableValue variableValue = FuzzyRules.FuzzyController.GetInputVariableValues().Where(x => x.variableGuid == variable.guid).ToList()[variableValueIndex];
                ((GraphNode)this.GetNode()).variableValueGuid = variableValue.guid;

                FuzzyRules.FuzzyController.SetInputGraphNode(FuzzyRules.Drive, GetNode().guid, (GraphNode)GetNode());
            } else if (this.graphNodeType == EEGraphNodeType.Output) {
                Variable variable = FuzzyRules.FuzzyController.GetOutputVariables().ToList()[variableIndex];
                VariableValue variableValue = FuzzyRules.FuzzyController.GetOutputVariableValues().Where(x => x.variableGuid == variable.guid).ToList()[variableValueIndex];
                ((GraphNode)this.GetNode()).variableValueGuid = variableValue.guid;

                FuzzyRules.FuzzyController.SetOutputGraphNode(FuzzyRules.Drive, GetNode().guid, (GraphNode)GetNode());
            }
        }

        Rect ranges = new Rect(0, 0, 1, 1);
        if (this.graphNodeType == EEGraphNodeType.Input) {
            if (variableIndex >= 0 && variableIndex < FuzzyRules.FuzzyController.GetInputVariables().Count()) {
                Variable variable = FuzzyRules.FuzzyController.GetInputVariables().ToList()[variableIndex];
                ranges.x = variable.lowerBound;
                ranges.width = variable.upperBound - variable.lowerBound;
            }
        } else if (this.graphNodeType == EEGraphNodeType.Output) {
            if (variableIndex >= 0 && variableIndex < FuzzyRules.FuzzyController.GetOutputVariables().Count()) {
                Variable variable = FuzzyRules.FuzzyController.GetOutputVariables().ToList()[variableIndex];
                ranges.x = variable.lowerBound;
                ranges.width = variable.upperBound - variable.lowerBound;
            }
        }

        if (!FuzzyRules.Drive.isMinView) {
            if (this.variableIndex < 0 || this.variableValueIndex < 0) {
                GUI.enabled = false;
            }
            EditorGUI.BeginChangeCheck();
            this.curve = EditorGUILayout.CurveField(this.curve, Color.white, ranges, GUILayout.Height(33));
            if (EditorGUI.EndChangeCheck()) {

                Variable variable = null;
                if (this.graphNodeType == EEGraphNodeType.Input) {
                    variable = FuzzyRules.FuzzyController.GetInputVariables().ToList()[variableIndex];
                } else if (this.graphNodeType == EEGraphNodeType.Output) {
                    variable = FuzzyRules.FuzzyController.GetOutputVariables().ToList()[variableIndex];
                }

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

                VariableValue variableValue = null;
                if (this.graphNodeType == EEGraphNodeType.Input) {
                    variableValue = FuzzyRules.FuzzyController.GetInputVariableValues().Where(x => x.variableGuid == variable.guid).ToList()[variableValueIndex];
                } else if (this.graphNodeType == EEGraphNodeType.Output) {
                    variableValue = FuzzyRules.FuzzyController.GetOutputVariableValues().Where(x => x.variableGuid == variable.guid).ToList()[variableValueIndex];
                }

                variableValue.graphPoints = EEUtils.GetGraphPoints(this.curve);
                variableValue.graphSamples = EEUtils.GetGraphSamples(variable, this.curve);

                FuzzyRules.FuzzyController.SetVariableValue(variableValue.guid, variableValue);
            }
            if (this.variableIndex < 0 || this.variableValueIndex < 0) {
                GUI.enabled = true;
            }
        }

        GUILayout.EndVertical();

        GUILayout.Space(5);

        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.EndArea();
    }

    public EEGraphNodeType GetGraphNodeType() {
        return this.graphNodeType;
    }

}