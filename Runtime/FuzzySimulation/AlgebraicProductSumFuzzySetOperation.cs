using System.Collections.Generic;
using System;

public class AlgebraicProductSumFuzzySetOperations : FuzzySetOperations {

    public float? Union(List<NodeTree> nodes, int instance, bool conNull, Func<NodeTree, int, float?> CalcTree) {
        float? v = null;

        foreach (NodeTree node in nodes) {
            float? value = CalcTree(node, instance);
            if (value == null) {
                if (conNull) continue;
                else return null;
            }
            if (v == null) v = value;
            else v = v + value - v * value;
        }

        return v;
    }

    public float? Intersection(List<NodeTree> nodes, int instance, bool conNull, Func<NodeTree, int, float?> CalcTree) {
        float? v = null;

        foreach (NodeTree node in nodes) {
            float? value = CalcTree(node, instance);
            if (value == null) {
                if (conNull) continue;
                else return null;
            }
            if (v == null) v = value;
            else v *= value;
        }

        return v;
    }

}