using System.Collections.Generic;
using System.Linq;
using Configuration;
using Pieces;
using UnityEngine;

public class GameField
{
    private const char EmptySymbol = '-';
    private const char EmptySymbolAlt = ' ';
    private const char RandomSymbol = '*';

    private readonly PieceFactory pieceFactory;
    private readonly GameFieldView gameFieldView;

    private Piece[,] pieces;

    public GameField(PieceFactory pieceFactory, GameFieldView gameFieldView)
    {
        this.pieceFactory = pieceFactory;
        this.gameFieldView = gameFieldView;
    }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public void BuildField(LevelConfig levelConfig, PieceMappingsConfig pieceMappings)
    {
        gameFieldView.PaintField(levelConfig.Map);

        Width = levelConfig.HorizontalSize;
        Height = levelConfig.VerticalSize;
        pieces = new Piece[Width, Height];

        List<string> colorKeyPool = levelConfig.MaxRegularPieceTypes > 0
            ? pieceMappings.OrderedSymbols
                .Take(levelConfig.MaxRegularPieceTypes)
                .Select(symbol => pieceMappings.PieceKeysBySymbol[symbol])
                .ToList()
            : new List<string>();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                CreatePieceAt(x, y, levelConfig.Map[x, y], colorKeyPool, pieceMappings);
            }
        }

        gameFieldView.CenterCamera();
    }

    public Piece GetPiece(Vector2Int coordinate) => pieces[coordinate.x, coordinate.y];

    private void CreatePieceAt(int x, int y, char symbol, List<string> colorKeyPool, PieceMappingsConfig pieceMappings)
    {
        if (symbol == EmptySymbol || symbol == EmptySymbolAlt) return;

        string key = symbol == RandomSymbol ? PickRandomKey(x, y, colorKeyPool) : pieceMappings.PieceKeysBySymbol[symbol];

        Vector2Int coordinate = new(x, y);
        RegularPiece piece = pieceFactory.CreateRegularPiece(key, gameFieldView.PiecesParent);
        piece.Coordinate = coordinate;
        piece.SetPositionImmediate(gameFieldView.GetWorldPosition(coordinate));

        pieces[x, y] = piece;
    }

    private string PickRandomKey(int x, int y, List<string> colorKeyPool)
    {
        HashSet<string> forbiddenKeys = new();

        string leftMatchKey = GetMatchingPairKey(x - 1, y, x - 2, y);
        if (leftMatchKey != null) forbiddenKeys.Add(leftMatchKey);

        string topMatchKey = GetMatchingPairKey(x, y - 1, x, y - 2);
        if (topMatchKey != null) forbiddenKeys.Add(topMatchKey);

        List<string> candidates = colorKeyPool.Where(key => !forbiddenKeys.Contains(key)).ToList();
        if (candidates.Count == 0) candidates = colorKeyPool;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private string GetMatchingPairKey(int firstX, int firstY, int secondX, int secondY)
    {
        if (firstX < 0 || firstY < 0 || secondX < 0 || secondY < 0) return null;

        if (pieces[firstX, firstY] is not RegularPiece first) return null;
        if (pieces[secondX, secondY] is not RegularPiece second) return null;

        return first.Key == second.Key ? first.Key : null;
    }
}
