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

    private const bool ReloadConfigsBeforeLevelStart = true;

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
        StartLevelAsync(1).Forget();
    }

    private void RestartLevel()
    {
        RestartLevelAsync().Forget();

        async UniTask RestartLevelAsync()
        {
            LevelLifetimeScope previousLevel = currentLevel;
            await StartLevelAsync(CurrentLevelNumber);
            DestroyLevel(previousLevel);
        }
    }

    private void StartNextLevel()
    {
        StartNextLevelAsync().Forget();

        async UniTask StartNextLevelAsync()
        {
            LevelLifetimeScope previousLevel = currentLevel;

            bool doesNextLevelExist = gameConfiguration.LevelsListConfig.LevelNames.Count >= CurrentLevelNumber + 1;
            if(doesNextLevelExist)
            {
                await StartLevelAsync(CurrentLevelNumber + 1);
            }
            else
            {
                globalEventBroker.RaiseGameWin();
            }
            DestroyLevel(previousLevel);
        }
    }

    private async UniTask StartLevelAsync(int levelNumber)
    {
        if(ReloadConfigsBeforeLevelStart)
        {
            await gameConfiguration.LoadAllConfigsAsync();
        }
        StartLevel(levelNumber);
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
