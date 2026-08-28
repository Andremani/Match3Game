using System;
using System.Collections.Generic;
using Pieces;
using UnityEngine;

public class MatchChecker
{
    private const int MinLineLength = 3;

    private static readonly Vector2Int[] AdjacentDirections =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private readonly GameField gameField;

    public MatchChecker(GameField gameField)
    {
        this.gameField = gameField;
    }

    public event Action<IReadOnlyList<MatchGroup>> MatchSucceeded;
    public event Action MatchFailed;

    public IReadOnlyList<MatchGroup> CheckAt(Vector2Int coordinate)
    {
        return Report(FindAllMatches().FindAll(group => Contains(group, coordinate)));
    }

    public IReadOnlyList<MatchGroup> CheckWholeField()
    {
        return Report(FindAllMatches());
    }

    private IReadOnlyList<MatchGroup> Report(List<MatchGroup> groups)
    {
        if (groups.Count > 0) MatchSucceeded?.Invoke(groups);
        else MatchFailed?.Invoke();

        return groups;
    }

    private static bool Contains(MatchGroup group, Vector2Int coordinate)
    {
        foreach (Vector2Int cell in group.Cells)
        {
            if (cell == coordinate) return true;
        }

        return false;
    }

    private List<MatchGroup> FindAllMatches()
    {
        int width = gameField.Width;
        int height = gameField.Height;

        bool[,] matched = new bool[width, height];
        MarkLineRuns(matched, width, height, horizontal: true);
        MarkLineRuns(matched, width, height, horizontal: false);
        MarkSquares(matched, width, height);

        List<MatchGroup> groups = new();
        bool[,] visited = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!matched[x, y] || visited[x, y]) continue;

                List<Vector2Int> cells = FloodFill(matched, visited, x, y, width, height);
                MatchShape shape = ClassifyShape(cells);

                // A pure 2x2 square isn't an actionable match yet - there's no "plane" booster
                // to spawn, so it's treated the same as no match at all until that's built.
                if (shape == MatchShape.Square2x2) continue;

                string key = ((RegularPiece)gameField.GetPiece(cells[0])).Key;
                groups.Add(new MatchGroup(cells, key, shape));
            }
        }

        return groups;
    }

    private void MarkLineRuns(bool[,] matched, int width, int height, bool horizontal)
    {
        int outerCount = horizontal ? height : width;
        int innerCount = horizontal ? width : height;

        for (int outer = 0; outer < outerCount; outer++)
        {
            int runStart = 0;

            for (int inner = 1; inner <= innerCount; inner++)
            {
                string previousKey = GetKey(horizontal, outer, inner - 1, width, height);
                string currentKey = GetKey(horizontal, outer, inner, width, height);

                if (currentKey != null && currentKey == previousKey) continue;

                int runLength = inner - runStart;
                if (previousKey != null && runLength >= MinLineLength)
                {
                    for (int i = runStart; i < inner; i++)
                    {
                        Vector2Int cell = horizontal ? new Vector2Int(i, outer) : new Vector2Int(outer, i);
                        matched[cell.x, cell.y] = true;
                    }
                }

                runStart = inner;
            }
        }
    }

    private void MarkSquares(bool[,] matched, int width, int height)
    {
        for (int x = 0; x < width - 1; x++)
        {
            for (int y = 0; y < height - 1; y++)
            {
                string key = GetKey(new Vector2Int(x, y));
                if (key == null) continue;

                if (GetKey(new Vector2Int(x + 1, y)) != key) continue;
                if (GetKey(new Vector2Int(x, y + 1)) != key) continue;
                if (GetKey(new Vector2Int(x + 1, y + 1)) != key) continue;

                matched[x, y] = true;
                matched[x + 1, y] = true;
                matched[x, y + 1] = true;
                matched[x + 1, y + 1] = true;
            }
        }
    }

    private List<Vector2Int> FloodFill(bool[,] matched, bool[,] visited, int startX, int startY, int width, int height)
    {
        string key = GetKey(new Vector2Int(startX, startY));
        List<Vector2Int> cells = new();
        Queue<Vector2Int> queue = new();

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            cells.Add(current);

            foreach (Vector2Int direction in AdjacentDirections)
            {
                Vector2Int next = current + direction;
                if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height) continue;
                if (visited[next.x, next.y] || !matched[next.x, next.y]) continue;
                if (GetKey(next) != key) continue;

                visited[next.x, next.y] = true;
                queue.Enqueue(next);
            }
        }

        return cells;
    }

    private static MatchShape ClassifyShape(List<Vector2Int> cells)
    {
        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;

        foreach (Vector2Int cell in cells)
        {
            minX = Mathf.Min(minX, cell.x);
            maxX = Mathf.Max(maxX, cell.x);
            minY = Mathf.Min(minY, cell.y);
            maxY = Mathf.Max(maxY, cell.y);
        }

        if (cells.Count == 4 && maxX - minX == 1 && maxY - minY == 1) return MatchShape.Square2x2;

        if (minX == maxX || minY == maxY)
        {
            return cells.Count switch
            {
                3 => MatchShape.Line3,
                4 => MatchShape.Line4,
                _ => MatchShape.Line5Plus
            };
        }

        return MatchShape.Corner;
    }

    private string GetKey(bool horizontal, int outer, int inner, int width, int height)
    {
        int x = horizontal ? inner : outer;
        int y = horizontal ? outer : inner;
        if (x < 0 || x >= width || y < 0 || y >= height) return null;

        return GetKey(new Vector2Int(x, y));
    }

    private string GetKey(Vector2Int coordinate) => (gameField.GetPiece(coordinate) as RegularPiece)?.Key;
}
