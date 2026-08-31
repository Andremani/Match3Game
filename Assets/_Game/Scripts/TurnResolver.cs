using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Pieces;
using UnityEngine;

public class TurnResolver : IDisposable
{
    private const float SwapDuration = 0.4f;
    private const float CollapseDuration = 0.4f;

    private static readonly Dictionary<SwipeDirection, Vector2Int> DirectionOffsets = new()
    {
        { SwipeDirection.Up, new Vector2Int(0, -1) },
        { SwipeDirection.Down, new Vector2Int(0, 1) },
        { SwipeDirection.Left, new Vector2Int(-1, 0) },
        { SwipeDirection.Right, new Vector2Int(1, 0) }
    };

    private readonly GameField gameField;
    private readonly MatchChecker matchChecker;
    private readonly FallManager fallManager;
    private readonly PieceSwipeHandler pieceSwipeHandler;

    private int turnsMade;
    private bool isBusy;
    private bool isInputEnabled = true;

    public TurnResolver(GameField gameField, MatchChecker matchChecker, FallManager fallManager, PieceSwipeHandler pieceSwipeHandler)
    {
        this.gameField = gameField;
        this.matchChecker = matchChecker;
        this.fallManager = fallManager;
        this.pieceSwipeHandler = pieceSwipeHandler;

        pieceSwipeHandler.Swiped += OnSwiped;
    }

    public void Dispose()
    {
        pieceSwipeHandler.Swiped -= OnSwiped;
    }

    public event Action<int> TurnMade;
    public event Action<string, int> PiecesCollected;
    public event Action TurnResolved;

    public event Action<IReadOnlyList<MatchGroup>> MatchSucceeded;
    public event Action MatchFailed;

    public void SetInputEnabled(bool enabled) => isInputEnabled = enabled;

    private void OnSwiped(Piece piece, SwipeDirection direction)
    {
        if (!isInputEnabled || isBusy) return;

        Vector2Int fromCoordinate = piece.Coordinate;
        Vector2Int toCoordinate = fromCoordinate + DirectionOffsets[direction];

        if (!gameField.IsValidPosition(toCoordinate)) return;

        Piece otherPiece = gameField.GetPiece(toCoordinate);
        if (otherPiece == null) return;

        ResolveSwipeAsync(piece, fromCoordinate, otherPiece, toCoordinate).Forget();
    }

    private async UniTaskVoid ResolveSwipeAsync(Piece piece, Vector2Int fromCoordinate, Piece otherPiece, Vector2Int toCoordinate)
    {
        isBusy = true;

        try
        {
            await SwapAsync(piece, fromCoordinate, otherPiece, toCoordinate);

            IReadOnlyList<MatchGroup> matches = matchChecker.CheckNeighbouringPair(fromCoordinate, toCoordinate);

            if (matches.Count > 0)
            {
                MatchSucceeded?.Invoke(matches);
            }
            else
            {
                MatchFailed?.Invoke();
                await SwapAsync(piece, toCoordinate, otherPiece, fromCoordinate);
                return;
            }

            turnsMade++;
            TurnMade?.Invoke(turnsMade);

            while (matches.Count > 0)
            {
                List<Vector2Int> emptyCells = await RemoveMatchesAsync(matches);
                List<Vector2Int> fallenPieces = await fallManager.ResolveFallsAsync(emptyCells);
                matches = matchChecker.CheckAt(fallenPieces);

                if (matches.Count > 0)
                {
                    MatchSucceeded?.Invoke(matches);
                }
            }

            //Todo: Handle situations when there are no matches on the field (softlock)

            //potentialMatches = matchChecker.PotentialMatches()
            //if (potentialMatches.Count == 0)
            //{
                //Reshuffle gamefield
            //}

            TurnResolved?.Invoke();
        }
        finally
        {
            isBusy = false;
        }
    }

    private async UniTask SwapAsync(Piece firstPiece, Vector2Int firstCoordinate, Piece secondPiece, Vector2Int secondCoordinate)
    {
        gameField.SetPiece(firstCoordinate, secondPiece);
        gameField.SetPiece(secondCoordinate, firstPiece);

        await UniTask.WhenAll(
            firstPiece.MoveToAsync(gameField.GetWorldPosition(secondCoordinate), SwapDuration),
            secondPiece.MoveToAsync(gameField.GetWorldPosition(firstCoordinate), SwapDuration));
    }

    private async UniTask<List<Vector2Int>> RemoveMatchesAsync(IReadOnlyList<MatchGroup> matches)
    {
        Dictionary<string, int> collectedAmounts = new();
        List<Piece> pieces = new();
        List<Vector2Int> emptyCells = new();

        foreach (MatchGroup group in matches)
        {
            foreach (Vector2Int cell in group.Cells)
            {
                Piece piece = gameField.GetPiece(cell);
                if (piece == null || pieces.Contains(piece)) continue;

                pieces.Add(piece);
            }
        }

        void OnMatchResolved(Piece piece)
        {
            emptyCells.Add(piece.Coordinate);
            gameField.RemovePiece(piece.Coordinate);

            string regularPieceType = (piece as RegularPiece)?.RegularPieceType;
            if (regularPieceType == null) return;

            collectedAmounts.TryGetValue(regularPieceType, out int amount);
            collectedAmounts[regularPieceType] = amount + 1;
        }

        foreach (Piece piece in pieces)
        {
            piece.MatchResolved += OnMatchResolved;
        }

        await UniTask.WhenAll(pieces.Select(piece => piece.ResolveMatchAsync(CollapseDuration)));

        foreach (Piece piece in pieces)
        {
            piece.MatchResolved -= OnMatchResolved;
        }

        foreach ((string key, int amount) in collectedAmounts)
        {
            PiecesCollected?.Invoke(key, amount);
        }

        return emptyCells;
    }
}
