using System.Collections.Generic;
using System;

public interface FuzzySetOperations {

    float? Union(List<NodeTree> nodes, Func<NodeTree, float?> CalcTree);
    float? Intersection(List<NodeTree> nodes, Func<NodeTree, float?> CalcTree);

}