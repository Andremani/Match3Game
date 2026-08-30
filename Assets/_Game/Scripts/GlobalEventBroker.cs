using System;
using System.Collections.Generic;

public class GlobalEventBroker
{
    public event Action<string> PieceMatchResolved;
    public event Action LevelWon;
    public event Action LevelLost;
    public event Action TurnMade;
    public event Action<string, int> PiecesCollected;
    public event Action<IReadOnlyList<MatchGroup>> MatchSucceeded;
    public event Action MatchFailed;

    public void RaisePieceMatchResolved(string pieceKey) => PieceMatchResolved?.Invoke(pieceKey);
    public void RaiseLevelWon() => LevelWon?.Invoke();
    public void RaiseLevelLost() => LevelLost?.Invoke();
    public void RaiseTurnMade() => TurnMade?.Invoke();
    public void RaisePiecesCollected(string pieceKey, int amount) => PiecesCollected?.Invoke(pieceKey, amount);
    public void RaiseMatchSucceeded(IReadOnlyList<MatchGroup> matches) => MatchSucceeded?.Invoke(matches);
    public void RaiseMatchFailed() => MatchFailed?.Invoke();
}
