using System;
using Configuration;
using UnityEngine;
using VContainer;

public class LevelManager : MonoBehaviour
{
    private GameField gameField;
    private TurnResolver turnResolver;
    private LevelConfig levelConfig;
    private PieceMappingsConfig pieceMappings;

    private string winPieceKey;
    private int turnsRemaining;
    private int collectedWinPieceCount;
    private bool isLevelEnded;

    public event Action LevelWon;
    public event Action LevelLost;

    [Inject]
    public void Construct(GameField gameField, TurnResolver turnResolver, LevelConfig levelConfig, PieceMappingsConfig pieceMappings)
    {
        this.gameField = gameField;
        this.turnResolver = turnResolver;
        this.levelConfig = levelConfig;
        this.pieceMappings = pieceMappings;
    }

    private void Start()
    {
        winPieceKey = pieceMappings.PieceKeysBySymbol[levelConfig.WinPieceType];
        turnsRemaining = levelConfig.MaxTurns;

        gameField.BuildField(levelConfig, pieceMappings);

        turnResolver.TurnMade += RegisterTurnMade;
        turnResolver.PiecesCollected += RegisterPieceCollected;
        turnResolver.TurnResolved += CheckOutcome;

        Debug.Log(
            $"Level started: {levelConfig.HorizontalSize}x{levelConfig.VerticalSize}, " +
            $"win target: collect {levelConfig.WinPieceAmount}x '{winPieceKey}', max turns: {turnsRemaining}");
    }

    public void RegisterPieceCollected(string pieceKey, int amount)
    {
        if (pieceKey != winPieceKey) return;

        collectedWinPieceCount += amount;
    }

    public void RegisterTurnMade(int turnNumber)
    {
        if (levelConfig.MaxTurns <= 0) return;

        turnsRemaining--;
    }

    public void CheckOutcome()
    {
        CheckWinCondition();
        CheckLoseCondition();
    }

    private void CheckWinCondition()
    {
        if (isLevelEnded || collectedWinPieceCount < levelConfig.WinPieceAmount) return;

        isLevelEnded = true;
        turnResolver.SetInputEnabled(false);

        Debug.Log("Level won");
        LevelWon?.Invoke();
    }

    private void CheckLoseCondition()
    {
        if (isLevelEnded || turnsRemaining > 0) return;

        isLevelEnded = true;
        turnResolver.SetInputEnabled(false);

        Debug.Log("Level lost");
        LevelLost?.Invoke();
    }
}
