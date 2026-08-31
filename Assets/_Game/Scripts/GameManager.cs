using System.Threading;
using Configuration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameManager : IAsyncStartable, System.IDisposable
{
    private LevelLifetimeScope levelLifetimeScopePrefab;
    private LevelLifetimeScope currentLevel;
    private GameConfiguration gameConfiguration;
    private GlobalEventBroker globalEventBroker;

    public int CurrentLevelNumber { get; private set; }

    public GameManager(LevelLifetimeScope levelLifetimeScopePrefab, GameConfiguration gameConfiguration, GlobalEventBroker globalEventBroker)
    {
        this.levelLifetimeScopePrefab = levelLifetimeScopePrefab;
        this.gameConfiguration = gameConfiguration;
        this.globalEventBroker = globalEventBroker;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        globalEventBroker.GameStartRequest += StartFirstLevel;
        globalEventBroker.RestartRequest += RestartLevel;
        globalEventBroker.NextLevelRequest += StartNextLevel;

        await gameConfiguration.LoadAllConfigsAsync();
    }

    public void Dispose()
    {
        globalEventBroker.GameStartRequest -= StartFirstLevel;
        globalEventBroker.RestartRequest -= RestartLevel;
        globalEventBroker.NextLevelRequest -= StartNextLevel;
    }

    private void StartFirstLevel()
    {
        if (currentLevel != null) return;
        StartLevel(1);
    }

    private void RestartLevel()
    {
        DestroyLevel(currentLevel);
        StartLevel(CurrentLevelNumber);
    }

    private void StartNextLevel()
    {
        DestroyLevel(currentLevel);
        StartLevel(CurrentLevelNumber + 1);
    }

    private void StartLevel(int levelNumber)
    {
        CurrentLevelNumber = levelNumber;

        string levelName = gameConfiguration.LevelsListConfig.LevelNames[levelNumber-1];

        LevelConfig levelConfig = gameConfiguration.LevelConfigs[levelName];

        using (LifetimeScope.Enqueue(builder =>
        {
            builder.RegisterInstance(gameConfiguration.PieceMappingsConfig);
            builder.RegisterInstance(levelConfig);
        }))
        currentLevel = Object.Instantiate(levelLifetimeScopePrefab);
        currentLevel.gameObject.name = $"Level '{levelName}'";

        globalEventBroker.RaiseLevelStarted(levelNumber, levelConfig);
    }

    private void DestroyLevel(LevelLifetimeScope levelLifetimeScope)
    {
        if (levelLifetimeScope != null)
        {
            levelLifetimeScope.Dispose(); 
            Object.Destroy(levelLifetimeScope.gameObject);
        }
    }
}
