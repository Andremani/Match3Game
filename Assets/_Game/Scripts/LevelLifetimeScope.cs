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

        builder.RegisterComponentOnNewGameObject<SwipeInputReader>(Lifetime.Scoped, "SwipeInputReader");
        builder.RegisterComponentOnNewGameObject<LevelManager>(Lifetime.Scoped, "LevelManager");
        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<SwipeInputReader>();
            container.Resolve<LevelManager>();
        });
    }
}
