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

    private List<string> regularPiecesTypePool = new();
    private int width;
    private int height;

    public GameField(PieceFactory pieceFactory, GameFieldView gameFieldView, MatchChecker matchChecker)
    {
        this.pieceFactory = pieceFactory;
        this.gameFieldView = gameFieldView;
        this.matchChecker = matchChecker;
        matchChecker.Init(this);
    }

    public Dictionary<Vector2Int, Cell> Cells { get; private set; } = new();
    public int Width => width;
    public int Height => height;
    public int CellsCount => Cells == null ? 0 : Cells.Count;

    public void BuildField(LevelConfig levelConfig, PieceMappingsConfig pieceMappings)
    {
        width = levelConfig.HorizontalSize;
        height = levelConfig.VerticalSize;
        if(levelConfig.MaxRegularPieceTypes > 0)
        {
            regularPiecesTypePool = pieceMappings.OrderedSymbols
                .Take(levelConfig.MaxRegularPieceTypes)
                .Select(symbol => pieceMappings.PieceKeysBySymbol[symbol])
                .ToList();
        }

        CreateCells(levelConfig);
        gameFieldView.PaintField(Cells, height, width);

        FillCells(levelConfig.Map, pieceMappings);
        gameFieldView.CenterCamera();
    }

    private void CreateCells(LevelConfig levelConfig)
    {
        Cells = new();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                char symbol = levelConfig.Map[x, y];
                if (!PieceMappingsConfig.IsEmptySymbol(symbol))
                {
                    Vector2Int coordinate = new Vector2Int(x,y);
                    Cells.Add(coordinate, new Cell(coordinate));
                }
            }
        }
    }

    private void FillCells(char[,] map, PieceMappingsConfig pieceMappings)
    {
        //Predefined pieces first, random after - for proper match-cheking

        foreach(var cell in Cells.Values)
        {
            char symbol = map[cell.Coordinate.x, cell.Coordinate.y];
            if (PieceMappingsConfig.RandomSymbol != symbol)
            {
                CreatePieceAt(cell.Coordinate, symbol, pieceMappings);
            }
        }

        foreach(var cell in Cells.Values)
        {
            if (cell.Piece != null) continue;

            char symbol = map[cell.Coordinate.x, cell.Coordinate.y];
            if (PieceMappingsConfig.RandomSymbol == symbol)
            {
                CreatePieceAt(cell.Coordinate, symbol, pieceMappings);
            }   
        }
    }

    public bool IsValidPosition(Vector2Int coordinate) => Cells.ContainsKey(coordinate);
    
    public Vector3 GetWorldPosition(Vector2Int coordinate) => gameFieldView.GetWorldPosition(coordinate);

    public Vector2 CellSize => gameFieldView.CellSize;

    public RegularPiece CreateRandomRegularPiece() =>
        pieceFactory.CreateRandomRegularPiece(regularPiecesTypePool, gameFieldView.PiecesParent);

    public Piece GetPiece(Vector2Int coordinate)
    {
        if(!IsValidPosition(coordinate)) return null;
        return Cells[coordinate].Piece;
    }

    public void SetPiece(Vector2Int coordinate, Piece piece)
    {
        if(!IsValidPosition(coordinate)) return;
        Cells[coordinate].Piece = piece;
        if (piece != null) piece.Coordinate = coordinate;
    }

    public void RemovePiece(Vector2Int coordinate)
    {
        if(!IsValidPosition(coordinate)) return;
        Cells[coordinate].Piece = null;
    }

    private void CreatePieceAt(Vector2Int coordinate, char symbol, PieceMappingsConfig pieceMappings)
    {
        if (PieceMappingsConfig.IsEmptySymbol(symbol)) return;

        string pieceTypeToCreate;
        if(symbol == PieceMappingsConfig.RandomSymbol)
        {
            pieceTypeToCreate = PickRandomRegularPieceKeyWithoutMatch(coordinate);
        }
        else
        {
            pieceTypeToCreate = pieceMappings.PieceKeysBySymbol[symbol];
        }

        RegularPiece piece = pieceFactory.CreateRegularPiece(pieceTypeToCreate, gameFieldView.PiecesParent);
        piece.Coordinate = coordinate;
        piece.SetPositionImmediate(gameFieldView.GetWorldPosition(coordinate));

        Cells[coordinate].Piece = piece;
    }

    private string PickRandomRegularPieceKeyWithoutMatch(Vector2Int coordinate)
    {
        List<string> candidatesTypePool = new(regularPiecesTypePool);
        string chosenType;
        bool hasMatch = false;

        do
        {
            chosenType = candidatesTypePool[Random.Range(0, candidatesTypePool.Count)];
            candidatesTypePool.Remove(chosenType);

            hasMatch = matchChecker.CheckAt(coordinate, chosenType).Count > 0;

        } while (hasMatch & candidatesTypePool.Count > 0);

        return chosenType;
    }
}
