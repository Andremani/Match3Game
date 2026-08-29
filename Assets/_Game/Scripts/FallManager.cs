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

    public event Action<Piece> PieceRegistered;
    public event Action FallsCompleted;

    public async UniTask ResolveFallsAsync(List<Vector2Int> emptyCells)
    {
        List<UniTask> fallTasks = new();
        IEnumerable<int> affectedColumns = emptyCells.Select(c => c.x).Distinct();

        foreach (int x in affectedColumns)
        {
            fallTasks.AddRange(ResolveColumn(x));
        }

        await UniTask.WhenAll(fallTasks);
        FallsCompleted?.Invoke();
    }

    private List<UniTask> ResolveColumn(int x)
    {
        List<Vector2Int> columnCells = new();
        int cellsInColumn = gameField.CellsAmountByColumn.TryGetValue(x, out int count) ? count : 0;

        for (int y = 0; y < gameField.Height && columnCells.Count < cellsInColumn; y++)
        {
            Vector2Int coordinate = new(x, y);
            if (gameField.IsValidPosition(coordinate))
            {
                columnCells.Add(coordinate);
            }
        }

        List<Piece> existingPieces = new();
        foreach (var coordinate in columnCells)
        {
            Piece piece = gameField.GetPiece(coordinate);
            if (piece == null) continue;

            existingPieces.Add(piece);
            gameField.RemovePiece(coordinate);
        }

        int emptyCount = columnCells.Count - existingPieces.Count;
        List<UniTask> fallTasks = new();

        for (int i = 0; i < existingPieces.Count; i++)
        {
            Vector2Int targetCoordinate = columnCells[emptyCount + i];
            fallTasks.Add(MovePieceAsync(existingPieces[i], targetCoordinate));
        }

        if (emptyCount > 0)
        {
            int topRow = columnCells[0].y;

            for (int i = 0; i < emptyCount; i++)
            {
                Vector2Int targetCoordinate = columnCells[i];
                Vector2Int spawnCoordinate = new(x, topRow - emptyCount + i);

                RegularPiece piece = gameField.CreateRandomRegularPiece();
                piece.Coordinate = spawnCoordinate;
                piece.SetPositionImmediate(gameField.GetWorldPosition(spawnCoordinate));

                fallTasks.Add(MovePieceAsync(piece, targetCoordinate));
            }
        }

        return fallTasks;
    }

    private async UniTask MovePieceAsync(Piece piece, Vector2Int targetCoordinate)
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
        PieceRegistered?.Invoke(piece);
    }
}
