using System.Collections.Generic;
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
}
