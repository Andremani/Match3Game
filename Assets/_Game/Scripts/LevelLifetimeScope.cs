using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;
using VContainer.Unity;

public class LevelLifetimeScope : LifetimeScope
{
    [SerializeField] private Tilemap tilemap;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(tilemap);
    }
}
