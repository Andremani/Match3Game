using System;
using System.Collections.Generic;
using Configuration;

public class GlobalEventBroker
{
    public event Action<string> PieceMatchResolved;
    public event Action GameStartRequest;
    public event Action RestartRequest;
    public event Action NextLevelRequest;
    public event Action<int, LevelConfig> LevelStarted;
    public event Action LevelWon;
    public event Action LevelLost;
    public event Action<int> TurnMade;
    public event Action<string, int> PiecesCollected;
    public event Action<IReadOnlyList<MatchGroup>> MatchSucceeded;
    public event Action MatchFailed;

    public void RaisePieceMatchResolved(string pieceKey) => PieceMatchResolved?.Invoke(pieceKey);
    public void RaiseGameStartRequest() => GameStartRequest?.Invoke();
    public void RaiseRestartRequest() => RestartRequest?.Invoke();
    public void RaiseNextLevelRequest() => NextLevelRequest?.Invoke();
    public void RaiseLevelStarted(int levelNumber, LevelConfig levelConfig) => LevelStarted?.Invoke(levelNumber, levelConfig);
    public void RaiseLevelWon() => LevelWon?.Invoke();
    public void RaiseLevelLost() => LevelLost?.Invoke();
    public void RaiseTurnMade(int turnNumber) => TurnMade?.Invoke(turnNumber);
    public void RaisePiecesCollected(string pieceKey, int amount) => PiecesCollected?.Invoke(pieceKey, amount);
    public void RaiseMatchSucceeded(IReadOnlyList<MatchGroup> matches) => MatchSucceeded?.Invoke(matches);
    public void RaiseMatchFailed() => MatchFailed?.Invoke();
}
