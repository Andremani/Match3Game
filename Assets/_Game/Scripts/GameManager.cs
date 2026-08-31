using System.Threading;
using Configuration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class GameManager : IAsyncStartable
{
    private InputAction startAction;
    private LevelLifetimeScope levelLifetimeScopePrefab;
    private LevelLifetimeScope currentLevel;
    private GameConfiguration gameConfiguration;
    private GlobalEventBroker globalEventBroker;

    public int CurrentLevelNumber { get; private set; }

    public event System.Action<int, LevelConfig> LevelStarted;

    public GameManager(LevelLifetimeScope levelLifetimeScopePrefab, GameConfiguration gameConfiguration, GlobalEventBroker globalEventBroker)
    {
        this.levelLifetimeScopePrefab = levelLifetimeScopePrefab;
        this.gameConfiguration = gameConfiguration;
        this.globalEventBroker = globalEventBroker;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        await gameConfiguration.LoadAllConfigsAsync();

        startAction = InputSystem.actions.FindAction("Submit");
        startAction.canceled += OnStartGameAction;

        globalEventBroker.LevelWon += OnLevelWon;
        globalEventBroker.LevelLost += OnLevelLost;
    }

    private void OnStartGameAction(InputAction.CallbackContext callback) => StartFirstLevel();

    private void StartFirstLevel()
    {
        if (currentLevel != null) return;
        StartLevel(1);
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

        LevelStarted?.Invoke(levelNumber, levelConfig);
    }

    private void OnLevelWon()
    {
        DestroyLevel(currentLevel);
        StartLevel(CurrentLevelNumber + 1);
    }

    private void OnLevelLost()
    {
        DestroyLevel(currentLevel);
        StartLevel(CurrentLevelNumber);
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
