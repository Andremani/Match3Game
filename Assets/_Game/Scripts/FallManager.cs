using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Pieces;
using UnityEngine;

public class FallManager
{
    private const float FallSpeed = 10f;

    private readonly GameField gameField;

    public FallManager(GameField gameField)
    {
        this.gameField = gameField;
    }

    public event Action<Piece> PieceFell;
    public event Action FallsCompleted;

    /// <returns>Fallen pieces coordinates</returns>
    public async UniTask<List<Vector2Int>> ResolveFallsAsync(List<Vector2Int> emptyCells)
    {
        List<UniTask<Vector2Int>> fallTasks = new();
        IEnumerable<int> affectedColumns = emptyCells.Select(c => c.x).Distinct();

        foreach (int x in affectedColumns)
        {
            fallTasks.AddRange(ResolveColumn(x));
        }

        Vector2Int[] results = await UniTask.WhenAll(fallTasks);
        List<Vector2Int> allFallenPieces = results.ToList();

        FallsCompleted?.Invoke();
        return allFallenPieces;
    }

    private List<UniTask<Vector2Int>> ResolveColumn(int x)
    {
        (List<Vector2Int> emptyCells, List<Piece> fallingPieces) = CollectGapsAndFallingPieces(x);

        List<UniTask<Vector2Int>> fallTasks = new();
        for (int i = 0; i < fallingPieces.Count; i++)
        {
            fallTasks.Add(MovePieceAsync(fallingPieces[i], emptyCells[i]));
        }

        List<Vector2Int> spawnTargets = emptyCells.Skip(fallingPieces.Count).Reverse().ToList();
        fallTasks.AddRange(SpawnFallingPieces(x, spawnTargets));

        return fallTasks;
    }

    // Scans the column bottom-to-top (y grows downward). Pieces already resting on the
    // floor are skipped entirely - they never moved. Once the first gap is found, every
    // cell above it falls: pieces go into fallingPieces, and every cell that ends up empty
    // (original gaps and cells vacated by falling pieces alike) is appended to emptyCells
    // in the same pass - so emptyCells[i] is already the correct target for
    // fallingPieces[i], and the leftover tail is what's left for newly spawned pieces.
    private (List<Vector2Int> emptyCells, List<Piece> fallingPieces) CollectGapsAndFallingPieces(int x)
    {
        List<Vector2Int> emptyCells = new();
        List<Piece> fallingPieces = new();
        bool gapFound = false;

        for (int y = gameField.Height - 1; y >= 0; y--)
        {
            Vector2Int coordinate = new(x, y);
            if (!gameField.IsValidPosition(coordinate)) continue;

            Piece piece = gameField.GetPiece(coordinate);

            if (!gapFound)
            {
                if (piece != null) continue;
                gapFound = true;
            }

            if (piece == null)
            {
                emptyCells.Add(coordinate);
            }
            else
            {
                fallingPieces.Add(piece);
                gameField.RemovePiece(coordinate);
                emptyCells.Add(coordinate);
            }
        }

        return (emptyCells, fallingPieces);
    }

    private List<UniTask<Vector2Int>> SpawnFallingPieces(int x, List<Vector2Int> targets)
    {
        List<UniTask<Vector2Int>> fallTasks = new();
        if (targets.Count == 0) return fallTasks;

        int topRow = targets[0].y;

        for (int i = 0; i < targets.Count; i++)
        {
            Vector2Int targetCoordinate = targets[i];
            Vector2Int spawnCoordinate = new(x, topRow - targets.Count + i);

            RegularPiece piece = gameField.CreateRandomRegularPiece();
            piece.Coordinate = spawnCoordinate;
            piece.SetPositionImmediate(gameField.GetWorldPosition(spawnCoordinate));

            fallTasks.Add(MovePieceAsync(piece, targetCoordinate));
        }

        return fallTasks;
    }

    private async UniTask<Vector2Int> MovePieceAsync(Piece piece, Vector2Int targetCoordinate)
    {
        Vector3 targetWorldPosition = gameField.GetWorldPosition(targetCoordinate);

        if (piece.Coordinate != targetCoordinate)
        {
            float distance = Vector3.Distance(piece.View.transform.position, targetWorldPosition);
            UniTask moveTask = piece.MoveToAsync(targetWorldPosition, distance / FallSpeed);
            piece.View.MaskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            await moveTask;
        }

        gameField.SetPiece(targetCoordinate, piece);
        PieceFell?.Invoke(piece);
        return targetCoordinate;
    }
}
