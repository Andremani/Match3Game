using Configuration;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private LevelLifetimeScope levelLifetimeScopePrefab;

    public LevelLifetimeScope LevelLifetimeScopePrefab => levelLifetimeScopePrefab;

    protected override void Configure(IContainerBuilder builder)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        builder.Register<IConfigFileReader, WebGLConfigFileReader>(Lifetime.Singleton);
#else
        builder.Register<IConfigFileReader, FileSystemConfigFileReader>(Lifetime.Singleton);
#endif
        builder.Register<ConfigParser>(Lifetime.Singleton);
        builder.Register<ConfigReader>(Lifetime.Singleton);

        builder.RegisterComponentOnNewGameObject<GameManager>(Lifetime.Singleton, "GameManager");
        builder.RegisterBuildCallback(container => container.Resolve<GameManager>());
    }
}
