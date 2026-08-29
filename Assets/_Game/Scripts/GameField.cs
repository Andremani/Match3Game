using System.Collections.Generic;
using System.Linq;
using Configuration;
using Pieces;
using UnityEngine;

public class GameField
{
    private readonly PieceFactory pieceFactory;
    private readonly GameFieldView gameFieldView;
    private readonly MatchChecker matchChecker;

    private Dictionary<Vector2Int, Cell> cells;
    private List<string> regularPiecesKeyPool = new();
    private int width;
    private int height;

    public GameField(PieceFactory pieceFactory, GameFieldView gameFieldView, MatchChecker matchChecker)
    {
        this.pieceFactory = pieceFactory;
        this.gameFieldView = gameFieldView;
        this.matchChecker = matchChecker;
        matchChecker.Init(this);
    }

    public int Width => width;
    public int Height => height;
    public Dictionary<int, int> CellsAmountByColumn = new();

    public void BuildField(LevelConfig levelConfig, PieceMappingsConfig pieceMappings)
    {
        width = levelConfig.HorizontalSize;
        height = levelConfig.VerticalSize;
        if(levelConfig.MaxRegularPieceTypes > 0)
        {
            regularPiecesKeyPool = pieceMappings.OrderedSymbols
                .Take(levelConfig.MaxRegularPieceTypes)
                .Select(symbol => pieceMappings.PieceKeysBySymbol[symbol])
                .ToList();
        }

        cells = CreateCells(levelConfig);
        gameFieldView.PaintField(cells, height, width);

        FillCells(levelConfig.Map, pieceMappings);
        gameFieldView.CenterCamera();
    }

    private Dictionary<Vector2Int, Cell> CreateCells(LevelConfig levelConfig)
    {
        Dictionary<Vector2Int, Cell> cells = new();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                char symbol = levelConfig.Map[x, y];
                if (!PieceMappingsConfig.IsEmptySymbol(symbol))
                {
                    Vector2Int coordinate = new Vector2Int(x,y);
                    cells.Add(coordinate, new Cell(coordinate));
                    
                    CellsAmountByColumn.TryGetValue(x, out int current);
                    CellsAmountByColumn[x] = current + 1;
                }
            }
        }
        return cells;
    }

    private void FillCells(char[,] map, PieceMappingsConfig pieceMappings)
    {
        //Predefined pieces first, random after - for proper match-cheking

        foreach(var cell in cells.Values)
        {
            char symbol = map[cell.Coordinate.x, cell.Coordinate.y];
            if (PieceMappingsConfig.RandomSymbol != symbol)
            {
                CreatePieceAt(cell.Coordinate, symbol, pieceMappings);
            }
        }

        foreach(var cell in cells.Values)
        {
            if (cell.Piece != null) continue;

            char symbol = map[cell.Coordinate.x, cell.Coordinate.y];
            if (PieceMappingsConfig.RandomSymbol == symbol)
            {
                CreatePieceAt(cell.Coordinate, symbol, pieceMappings);
            }   
        }
    }

    public bool IsValidPosition(Vector2Int coordinate) => cells.ContainsKey(coordinate);
    
    public Vector3 GetWorldPosition(Vector2Int coordinate) => gameFieldView.GetWorldPosition(coordinate);

    public Vector2 CellSize => gameFieldView.CellSize;

    public RegularPiece CreateRandomRegularPiece() =>
        pieceFactory.CreateRandomRegularPiece(regularPiecesKeyPool, gameFieldView.PiecesParent);

    public Piece GetPiece(Vector2Int coordinate)
    {
        if(!IsValidPosition(coordinate)) return null;
        return cells[coordinate].Piece;
    }

    public void SetPiece(Vector2Int coordinate, Piece piece)
    {
        if(!IsValidPosition(coordinate)) return;
        cells[coordinate].Piece = piece;
        if (piece != null) piece.Coordinate = coordinate;
    }

    public void RemovePiece(Vector2Int coordinate)
    {
        if(!IsValidPosition(coordinate)) return;
        cells[coordinate].Piece = null;
    }

    private void CreatePieceAt(Vector2Int coordinate, char symbol, PieceMappingsConfig pieceMappings)
    {
        if (PieceMappingsConfig.IsEmptySymbol(symbol)) return;

        RegularPiece piece;
        if(symbol == PieceMappingsConfig.RandomSymbol)
        {
            piece = CreateRandomRegularPieceWithoutMatch(coordinate);
        }
        else
        {
            string key = pieceMappings.PieceKeysBySymbol[symbol];
            piece = pieceFactory.CreateRegularPiece(key, gameFieldView.PiecesParent);
        }

        piece.Coordinate = coordinate;
        piece.SetPositionImmediate(gameFieldView.GetWorldPosition(coordinate));

        cells[coordinate].Piece = piece;
    }

    private RegularPiece CreateRandomRegularPieceWithoutMatch(Vector2Int coordinate)
    {
        // MatchChecker reads pieces straight from the field, so a candidate has to actually
        // be placed at the cell before CheckAt can tell whether it would form a match.
        List<string> candidatesKeyPool = new(regularPiecesKeyPool);

        while (true)
        {
            string key = candidatesKeyPool[Random.Range(0, candidatesKeyPool.Count)];
            candidatesKeyPool.Remove(key);

            RegularPiece piece = pieceFactory.CreateRegularPiece(key, gameFieldView.PiecesParent);
            cells[coordinate].Piece = piece;

            bool hasMatch = matchChecker.CheckAt(coordinate).Count > 0;
            if (!hasMatch || candidatesKeyPool.Count == 0)
            {
                cells[coordinate].Piece = null;
                return piece;
            }

            cells[coordinate].Piece = null;
            piece.Destroy();
        }
    }
}
