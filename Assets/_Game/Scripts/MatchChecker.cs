using System.Collections.Generic;
using System.Linq;
using Pieces;
using UnityEngine;

public class MatchChecker
{
    private const int MinLineLength = 3;
    private const float WholeFieldCheckCoefficient = 0.3f;
    private bool areSquaresAllowed = false;

    private static readonly Vector2Int[] AdjacentDirections =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private GameField gameField;

    public void Init(GameField gameField)
    {
        this.gameField = gameField;
    }

    public IReadOnlyList<MatchGroup> CheckNeighbouringPair(Vector2Int firstCoordinate, Vector2Int secondCoordinate)
    {
        IReadOnlyList<MatchGroup> matches;
        Piece firstPiece = gameField.GetPiece(firstCoordinate);
        Piece secondPiece = gameField.GetPiece(secondCoordinate);

        if(Piece.AreSame(firstPiece, secondPiece))
        {
            matches = CheckAt(firstCoordinate);
        }
        else
        {
            IReadOnlyList<MatchGroup> matches1 = CheckAt(firstCoordinate);
            IReadOnlyList<MatchGroup> matches2 = CheckAt(secondCoordinate);
            matches = matches1.Concat(matches2).ToList();
        }

        return matches;
    }

    public IReadOnlyList<MatchGroup> CheckAt(List<Vector2Int> coordinates)
    {
        List<MatchGroup> matches;

        if(coordinates.Count < gameField.CellsCount * WholeFieldCheckCoefficient)
        {
            matches = new();
            foreach(Vector2Int coordinate in coordinates)
            {
                matches.AddRange(CheckAt(coordinate));
            }
            matches = MatchGroup.GetUniqueGroups(matches);
        }
        else
        {
            matches = FindAllMatches();
        }
        return matches;
    }

    public IReadOnlyList<MatchGroup> CheckAt(Vector2Int coordinate) => CheckAt(coordinate, GetKey(coordinate));

    // Variant for a piece that isn't on the field yet - lets initial fill test a
    // candidate key at a coordinate without actually placing anything there first
    public IReadOnlyList<MatchGroup> CheckAt(Vector2Int coordinate, string key)
    {
        List<MatchGroup> groups = new();
        if (key == null) return groups;

        List<Vector2Int> horizontalRun = ExpandLine(coordinate, key, horizontal: true);
        List<Vector2Int> verticalRun = ExpandLine(coordinate, key, horizontal: false);

        List<Vector2Int> lineCells = new();
        if (horizontalRun.Count >= MinLineLength) lineCells.AddRange(horizontalRun);
        if (verticalRun.Count >= MinLineLength) lineCells.AddRange(verticalRun);

        if (lineCells.Count > 0)
        {
            List<Vector2Int> cells = lineCells.Distinct().ToList();
            MatchShape shape = ClassifyShape(cells);

            groups.Add(new MatchGroup(cells, key, shape));
        }

        if(areSquaresAllowed)
        {
            foreach (List<Vector2Int> square in FindSquaresAt(coordinate, key))
            {
                groups.Add(new MatchGroup(square, key, MatchShape.Square2x2));
            }
        }

        return groups;
    }

    private List<Vector2Int> ExpandLine(Vector2Int coordinate, string key, bool horizontal)
    {
        Vector2Int forward = horizontal ? Vector2Int.right : Vector2Int.up;
        Vector2Int backward = -forward;

        List<Vector2Int> cells = new() { coordinate };

        for (Vector2Int cell = coordinate + backward; Piece.AreSame(gameField.GetPiece(cell), key); cell += backward)
        {
            cells.Add(cell);
        }

        for (Vector2Int cell = coordinate + forward; Piece.AreSame(gameField.GetPiece(cell), key); cell += forward)
        {
            cells.Add(cell);
        }

        return cells;
    }

    private IEnumerable<List<Vector2Int>> FindSquaresAt(Vector2Int coordinate, string key)
    {
        Vector2Int[] corners =
        {
            coordinate + new Vector2Int(-1, -1),
            coordinate + new Vector2Int(0, -1),
            coordinate + new Vector2Int(-1, 0),
            coordinate
        };

        foreach (Vector2Int corner in corners)
        {
            List<Vector2Int> square = new()
            {
                corner,
                corner + Vector2Int.right,
                corner + Vector2Int.up,
                corner + Vector2Int.one
            };

            if (square.TrueForAll(cell => cell == coordinate || Piece.AreSame(gameField.GetPiece(cell), key)))
            {
                yield return square;
            }
        }
    }

    private List<MatchGroup> FindAllMatches()
    {
        int width = gameField.Width;
        int height = gameField.Height;

        bool[,] matched = new bool[width, height];
        MarkLineRuns(matched, width, height, horizontal: true);
        MarkLineRuns(matched, width, height, horizontal: false);
        if(areSquaresAllowed)
        {
            MarkSquares(matched, width, height);
        }

        List<MatchGroup> groups = new();
        bool[,] visited = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!matched[x, y] || visited[x, y]) continue;

                List<Vector2Int> cells = FloodFill(matched, visited, x, y, width, height);
                MatchShape shape = ClassifyShape(cells);

                if(!areSquaresAllowed)
                {
                    if (shape == MatchShape.Square2x2) continue;
                }

                string regularPieceType = ((RegularPiece)gameField.GetPiece(cells[0])).RegularPieceType;
                groups.Add(new MatchGroup(cells, regularPieceType, shape));
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

    private string GetKey(Vector2Int coordinate) => (gameField.GetPiece(coordinate) as RegularPiece)?.RegularPieceType;
}
