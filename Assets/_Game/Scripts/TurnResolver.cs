using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Pieces;
using UnityEngine;

public class TurnResolver
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
    private readonly SwipeInputReader swipeInputReader;

    private bool isBusy;
    private bool isInputEnabled = true;

    public TurnResolver(GameField gameField, MatchChecker matchChecker, FallManager fallManager, SwipeInputReader swipeInputReader)
    {
        this.gameField = gameField;
        this.matchChecker = matchChecker;
        this.fallManager = fallManager;
        this.swipeInputReader = swipeInputReader;

        swipeInputReader.Swiped += OnSwiped;
    }

    public event Action TurnMade;
    public event Action<string, int> PiecesCollected;
    public event Action TurnResolved;

    public void SetInputEnabled(bool enabled) => isInputEnabled = enabled;

    private async void OnSwiped(Piece piece, SwipeDirection direction)
    {
        if (!isInputEnabled || isBusy) return;

        Vector2Int fromCoordinate = piece.Coordinate;
        Vector2Int toCoordinate = fromCoordinate + DirectionOffsets[direction];

        if (!gameField.IsInBounds(toCoordinate)) return;

        Piece otherPiece = gameField.GetPiece(toCoordinate);
        if (otherPiece == null) return;

        isBusy = true;

        await SwapAsync(piece, fromCoordinate, otherPiece, toCoordinate);

        List<MatchGroup> matches = DeduceGroups(
            matchChecker.CheckAt(fromCoordinate),
            matchChecker.CheckAt(toCoordinate));

        if (matches.Count == 0)
        {
            await SwapAsync(piece, toCoordinate, otherPiece, fromCoordinate);
            isBusy = false;
            return;
        }

        TurnMade?.Invoke();

        while (matches.Count > 0)
        {
            await RemoveMatchesAsync(matches);
            await fallManager.ResolveFallsAsync();
            matches = matchChecker.CheckWholeField().ToList();
        }

        TurnResolved?.Invoke();
        isBusy = false;
    }

    private async UniTask SwapAsync(Piece firstPiece, Vector2Int firstCoordinate, Piece secondPiece, Vector2Int secondCoordinate)
    {
        gameField.SetPiece(firstCoordinate, secondPiece);
        gameField.SetPiece(secondCoordinate, firstPiece);

        await UniTask.WhenAll(
            firstPiece.MoveToAsync(gameField.GetWorldPosition(secondCoordinate), SwapDuration),
            secondPiece.MoveToAsync(gameField.GetWorldPosition(firstCoordinate), SwapDuration));
    }

    private async UniTask RemoveMatchesAsync(List<MatchGroup> matches)
    {
        Dictionary<string, int> collectedAmounts = new();
        List<Piece> pieces = new();

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
            gameField.RemovePiece(piece.Coordinate);

            string key = (piece as RegularPiece)?.Key;
            if (key == null) return;

            collectedAmounts.TryGetValue(key, out int amount);
            collectedAmounts[key] = amount + 1;
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
    }

    private static List<MatchGroup> DeduceGroups(IReadOnlyList<MatchGroup> first, IReadOnlyList<MatchGroup> second)
    {
        List<MatchGroup> groups = new(first);

        foreach (MatchGroup group in second)
        {
            bool overlaps = groups.Any(existing => existing.Cells.Intersect(group.Cells).Any());
            if (!overlaps) groups.Add(group);
        }

        return groups;
    }
}
