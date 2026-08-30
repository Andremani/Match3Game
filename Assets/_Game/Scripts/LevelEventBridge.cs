using System;
using Pieces;

public class LevelEventBridge : IDisposable
{
    private readonly LevelManager levelManager;
    private readonly TurnResolver turnResolver;
    private readonly PieceFactory pieceFactory;
    private readonly GlobalEventBroker globalEventBroker;

    public LevelEventBridge(LevelManager levelManager, TurnResolver turnResolver, PieceFactory pieceFactory, GlobalEventBroker globalEventBroker)
    {
        this.levelManager = levelManager;
        this.turnResolver = turnResolver;
        this.pieceFactory = pieceFactory;
        this.globalEventBroker = globalEventBroker;

        levelManager.LevelWon += globalEventBroker.RaiseLevelWon;
        levelManager.LevelLost += globalEventBroker.RaiseLevelLost;

        turnResolver.TurnMade += globalEventBroker.RaiseTurnMade;
        turnResolver.PiecesCollected += globalEventBroker.RaisePiecesCollected;
        turnResolver.MatchSucceeded += globalEventBroker.RaiseMatchSucceeded;
        turnResolver.MatchFailed += globalEventBroker.RaiseMatchFailed;

        pieceFactory.PieceCreated += OnPieceCreated;
    }

    public void Dispose()
    {
        levelManager.LevelWon -= globalEventBroker.RaiseLevelWon;
        levelManager.LevelLost -= globalEventBroker.RaiseLevelLost;

        turnResolver.TurnMade -= globalEventBroker.RaiseTurnMade;
        turnResolver.PiecesCollected -= globalEventBroker.RaisePiecesCollected;
        turnResolver.MatchSucceeded -= globalEventBroker.RaiseMatchSucceeded;
        turnResolver.MatchFailed -= globalEventBroker.RaiseMatchFailed;

        pieceFactory.PieceCreated -= OnPieceCreated;
    }

    private void OnPieceCreated(Piece piece) => piece.MatchResolved += OnPieceMatchResolved;

    private void OnPieceMatchResolved(Piece piece) => globalEventBroker.RaisePieceMatchResolved(piece.Key);
   
}
