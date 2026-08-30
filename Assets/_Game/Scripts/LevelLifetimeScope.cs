using Pieces;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelLifetimeScope : LifetimeScope
{
    [SerializeField] private PieceViewMappingConfig pieceViewMapping;
    [SerializeField] private GameFieldView gameFieldView;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(pieceViewMapping);
        builder.RegisterComponent(gameFieldView);

        builder.Register<PieceFactory>(Lifetime.Scoped);
        builder.Register<GameField>(Lifetime.Scoped);
        builder.Register<MatchChecker>(Lifetime.Scoped);
        builder.Register<FallManager>(Lifetime.Scoped);
        builder.Register<TurnResolver>(Lifetime.Scoped);
        builder.Register<PieceSwipeHandler>(Lifetime.Scoped);

        builder.RegisterComponentOnNewGameObject<LevelManager>(Lifetime.Scoped, "LevelManager");
        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<PieceSwipeHandler>();
            container.Resolve<TurnResolver>();
            container.Resolve<LevelManager>();
        });
    }
}
