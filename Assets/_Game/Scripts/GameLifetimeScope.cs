using Configuration;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private AudioPlayer audioPlayerPrefab;
    [SerializeField] private AudioDatabase audioDatabase;
    [SerializeField] private AudioManager audioManagerPrefab;
    [SerializeField] private LevelLifetimeScope levelLifetimeScopePrefab;

    protected override void Configure(IContainerBuilder builder)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        builder.Register<IConfigFileReader, WebGLConfigFileReader>(Lifetime.Singleton);
#else
        builder.Register<IConfigFileReader, FileSystemConfigFileReader>(Lifetime.Singleton);
#endif
        builder.Register<ConfigParser>(Lifetime.Singleton);
        builder.Register<ConfigReader>(Lifetime.Singleton);
        builder.Register<GameConfiguration>(Lifetime.Singleton);
        builder.Register<GlobalEventBroker>(Lifetime.Singleton);

        builder.RegisterComponentInNewPrefab(audioPlayerPrefab, Lifetime.Singleton).As<IAudioPlayer>();
        builder.RegisterInstance(audioDatabase);
        builder.RegisterComponentInNewPrefab(audioManagerPrefab, Lifetime.Singleton);

        builder.RegisterInstance(levelLifetimeScopePrefab);

        builder.RegisterEntryPoint<GameManager>(Lifetime.Singleton).AsSelf();
        builder.RegisterComponentOnNewGameObject<SwipeGestureDetector>(Lifetime.Singleton, "SwipeGestureDetector");
        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<SwipeGestureDetector>();
            container.Resolve<AudioPlayer>();
            container.Resolve<AudioManager>();
        });
    }
}
