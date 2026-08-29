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
        List<Vector2Int> lowestEmptyCellsInColumns = new();

        foreach(var cellCoordinate in emptyCells)
        {
            Vector2Int existing = lowestEmptyCellsInColumns.FirstOrDefault(c => c.x == cellCoordinate.x);

            if(existing == null)
            {
                lowestEmptyCellsInColumns.Add(cellCoordinate);
            }
            else
            {
                if(cellCoordinate.y > existing.y)
                {
                    lowestEmptyCellsInColumns.Remove(existing);
                    lowestEmptyCellsInColumns.Add(cellCoordinate);
                }
            }
        }

        for (int x = 0; x < gameField.Width; x++)
        {
            fallTasks.AddRange(ResolveColumn(x));
        }

        await UniTask.WhenAll(fallTasks);
        FallsCompleted?.Invoke();
    }

    private List<UniTask> ResolveColumn(int x)
    {
        List<Piece> existingPieces = new();

        for (int y = gameField.Height - 1; y >= 0; y--)
        {
            Vector2Int coordinate = new(x, y);
            Piece piece = gameField.GetPiece(coordinate);
            if (piece == null) continue;

            existingPieces.Add(piece);
            gameField.RemovePiece(coordinate);
        }

        int emptyCount = gameField.Height - existingPieces.Count;
        List<UniTask> fallTasks = new();

        for (int i = 0; i < existingPieces.Count; i++)
        {
            Vector2Int targetCoordinate = new(x, emptyCount + i);
            fallTasks.Add(MovePieceAsync(existingPieces[i], targetCoordinate));
        }

        for (int i = 0; i < emptyCount; i++)
        {
            Vector2Int targetCoordinate = new(x, i);
            Vector2Int spawnCoordinate = new(x, i - emptyCount);

            RegularPiece piece = gameField.CreateRandomRegularPiece();
            piece.Coordinate = spawnCoordinate;
            piece.SetPositionImmediate(gameField.GetWorldPosition(spawnCoordinate));

            fallTasks.Add(MovePieceAsync(piece, targetCoordinate));
        }

        return fallTasks;
    }

    private async UniTask MovePieceAsync(Piece piece, Vector2Int targetCoordinate)
    {
        Vector3 targetWorldPosition = gameField.GetWorldPosition(targetCoordinate);

        if (piece.Coordinate != targetCoordinate)
        {
            float distance = Vector3.Distance(piece.View.transform.position, targetWorldPosition);
            await piece.MoveToAsync(targetWorldPosition, distance / FallSpeed);
        }

        gameField.SetPiece(targetCoordinate, piece);
        PieceRegistered?.Invoke(piece);
    }
}
