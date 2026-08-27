using System;
using Configuration;
using UnityEngine;
using VContainer;

public class LevelManager : MonoBehaviour
{
    private GameField gameField;
    private LevelConfig levelConfig;
    private PieceMappingsConfig pieceMappings;

    private string winPieceKey;
    private int turnsRemaining;
    private int collectedWinPieceCount;

    public event Action LevelWon;
    public event Action LevelLost;

    [Inject]
    public void Construct(GameField gameField, LevelConfig levelConfig, PieceMappingsConfig pieceMappings)
    {
        this.gameField = gameField;
        this.levelConfig = levelConfig;
        this.pieceMappings = pieceMappings;
    }

    private void Start()
    {
        winPieceKey = pieceMappings.PieceKeysBySymbol[levelConfig.WinPieceType];
        turnsRemaining = levelConfig.MaxTurns;

        gameField.BuildField(levelConfig, pieceMappings);

        Debug.Log(
            $"Level started: {levelConfig.HorizontalSize}x{levelConfig.VerticalSize}, " +
            $"win target: collect {levelConfig.WinPieceAmount}x '{winPieceKey}', max turns: {turnsRemaining}");
    }

    public void RegisterPieceCollected(string pieceKey, int amount)
    {
        if (pieceKey != winPieceKey) return;

        collectedWinPieceCount += amount;
        Debug.Log($"Collected {collectedWinPieceCount}/{levelConfig.WinPieceAmount} '{winPieceKey}'");

        CheckWinCondition();
    }

    public void RegisterTurnMade()
    {
        if (levelConfig.MaxTurns <= 0) return;

        turnsRemaining--;
        CheckLoseCondition();
    }

    private void CheckWinCondition()
    {
        if (collectedWinPieceCount < levelConfig.WinPieceAmount) return;

        Debug.Log("Level won");
        LevelWon?.Invoke();
    }

    private void CheckLoseCondition()
    {
        if (turnsRemaining > 0) return;

        Debug.Log("Level lost");
        LevelLost?.Invoke();
    }
}
