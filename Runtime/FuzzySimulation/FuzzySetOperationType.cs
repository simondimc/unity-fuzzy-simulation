using System.Collections.Generic;
using System;

public interface FuzzySetOperations {

    float? Union(List<NodeTree> nodes, int instance, bool conNull, Func<NodeTree, int, float?> CalcTree);
    float? Intersection(List<NodeTree> nodes, int instance, bool conNull, Func<NodeTree, int, float?> CalcTree);

}