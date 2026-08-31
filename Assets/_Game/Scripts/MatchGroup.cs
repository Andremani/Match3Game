using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public readonly struct MatchGroup
{
    public MatchGroup(IReadOnlyList<Vector2Int> cells, string key, MatchShape shape)
    {
        Cells = cells;
        Key = key;
        Shape = shape;
    }

    public IReadOnlyList<Vector2Int> Cells { get; }
    public string Key { get; }
    public MatchShape Shape { get; }

    public static List<MatchGroup> GetUniqueGroups(List<MatchGroup> groups)
    {
        List<MatchGroup> uniqueGroups = new();
        foreach (var match in groups)
        {
            if (!uniqueGroups.Any(u => IsSameCellSet(u, match)))
            {
                uniqueGroups.Add(match);
            }
        }
        return uniqueGroups;
    }

    private static bool IsSameCellSet(MatchGroup first, MatchGroup second)
    {
        return IsSameCellSet(first.Cells, second.Cells);
    }

    private static bool IsSameCellSet(IReadOnlyList<Vector2Int> first, IReadOnlyList<Vector2Int> second)
    {
        if (first.Count != second.Count) return false;
        return first.All(second.Contains);
    }
}
