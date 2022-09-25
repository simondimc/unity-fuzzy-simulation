using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class EEUtils {

    public static void DrawLine(Color color) {
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;
        EditorGUI.DrawRect(rect, color);
    }

    public static Texture2D ColorTexture(Color color) {
        Texture2D texture = new Texture2D(1, 1);
        Color[] pixels = texture.GetPixels();
        for(var i = 0; i < pixels.Length; i++) {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    public static void SetKeyframes(AnimationCurve curve, List<GraphPoint> graphPoints) {
        Keyframe[] keyframes = new Keyframe[graphPoints.Count];
        for (int i = 0; i < graphPoints.Count; i++) {
            GraphPoint graphKey = graphPoints[i];
            keyframes[i] = new Keyframe(graphKey.x, graphKey.y, graphKey.inTangent, graphKey.outTangent, graphKey.inWeight, graphKey.outWeight);
            keyframes[i].weightedMode = (WeightedMode)graphKey.weightedMode;
        }
        curve.keys = keyframes;
        for (int i = 0; i < graphPoints.Count; i++) {
            GraphPoint graphKey = graphPoints[i];
            AnimationUtility.SetKeyLeftTangentMode(curve, i, (AnimationUtility.TangentMode)graphKey.leftTangentMode);
            AnimationUtility.SetKeyRightTangentMode(curve, i, (AnimationUtility.TangentMode)graphKey.rightTangentMode);
        }
    }

    public static List<GraphPoint> GetGraphPoints(AnimationCurve curve) {
        List<GraphPoint> graphPoints = new List<GraphPoint>();
        for (int i = 0; i < curve.keys.Length; i++) {
            Keyframe keyframe = curve.keys[i];

            int weightedMode = (int)keyframe.weightedMode;
            
            int leftTangentMode = (int)AnimationUtility.GetKeyLeftTangentMode(curve, i);
            int rightTangentMode = (int)AnimationUtility.GetKeyRightTangentMode(curve, i);
            

            graphPoints.Add(new GraphPoint(
                keyframe.time, 
                keyframe.value,
                keyframe.inTangent,
                keyframe.outTangent,
                keyframe.inWeight,
                keyframe.outWeight,
                weightedMode,
                leftTangentMode,
                rightTangentMode));
        }
        return graphPoints;
    }

    public static List<GraphSample> GetGraphSamples(Variable variable, AnimationCurve curve) {
        List<GraphSample> graphPoints = new List<GraphSample>();

        for (float x = variable.lowerBound; x <= variable.upperBound; x += (variable.upperBound - variable.lowerBound) / 256) {
            float y = curve.Evaluate(x);
            if (y < 0) y = 0;
            if (y > 1) y = 1;
            graphPoints.Add(new GraphSample(x, y));
        }

        return graphPoints;
    }

    public static void FixVariableValues(Variable variable) {
        List<VariableValue> variableValues = FuzzyRules.FuzzyController.GetVariableValues().ToList();

        foreach (VariableValue variableValue in variableValues) {
            EEUtils.FixVariableValue(variableValue);
        }
    }

    public static void FixVariableValue(VariableValue variableValue) {
        Variable variable = FuzzyRules.FuzzyController.GetVariables().ToList().Find(x => x.guid == variableValue.variableGuid);

        List<GraphPoint> graphPoints = variableValue.graphPoints;

        List<GraphPoint> toRemove = new List<GraphPoint>();
        bool hasLowerBound = false;
        bool hasUpperBound = false;

        foreach (GraphPoint graphPoint in graphPoints) {
            if (graphPoint.x < variable.lowerBound || graphPoint.x > variable.upperBound) {
                toRemove.Add(graphPoint);
            }
            if (graphPoint.x == variable.lowerBound) {
                hasLowerBound = true;
            }
            if (graphPoint.x == variable.upperBound) {
                hasUpperBound = true;
            }
        }

        graphPoints = graphPoints.Except(toRemove).ToList();

        AnimationCurve curve = new AnimationCurve();
        EEUtils.SetKeyframes(curve, graphPoints);

        if (!hasLowerBound) {
            int i = curve.AddKey(variable.lowerBound, 0);
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }
        if (!hasUpperBound) {
            int ub = 0;
            if (curve.keys.Length <= 1) ub = 1;
            int i = curve.AddKey(variable.upperBound, ub);
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }

        variableValue.graphPoints = EEUtils.GetGraphPoints(curve);
        variableValue.graphSamples = EEUtils.GetGraphSamples(variable, curve);

        FuzzyRules.FuzzyController.SetVariableValue(variableValue.guid, variableValue);
    }


}