using System.Collections.Generic;
using System;

public class MinimumMaximumFuzzySetOperations : FuzzySetOperations {

    public float? Union(List<NodeTree> nodes, Func<NodeTree, float?> CalcTree) {
        float? v = null;

        foreach (NodeTree node in nodes) {
            float? value = CalcTree(node);
            if (value == null) continue;
            if (v == null || value > v) v = value;
        }

        return v;
    }

    public float? Intersection(List<NodeTree> nodes, Func<NodeTree, float?> CalcTree) {
        float? v = null;

        foreach (NodeTree node in nodes) {
            float? value = CalcTree(node);
            if (value == null) continue;
            if (v == null || value < v) v = value;
        }

        return v;
    }

}