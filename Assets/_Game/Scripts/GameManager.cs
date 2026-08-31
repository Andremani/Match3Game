using System.Threading;
using System.Collections.Generic;
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
    private LevelLifetimeScope currentLevelScope;
    private GameConfiguration gameConfiguration;
    private GlobalEventBroker globalEventBroker;

    public string CurrentLevelName { get; private set; } = string.Empty;

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
        if (currentLevelScope != null) return;
        StartLevel(1);
    }

    private void StartLevel(int levelNumber)
    {
        CurrentLevelName = gameConfiguration.LevelsListConfig.LevelNames[levelNumber-1];

        LevelConfig levelConfig = gameConfiguration.LevelConfigs[CurrentLevelName];

        using (LifetimeScope.Enqueue(builder =>
        {
            builder.RegisterInstance(gameConfiguration.PieceMappingsConfig);
            builder.RegisterInstance(levelConfig);
        }))
        currentLevelScope = Object.Instantiate(levelLifetimeScopePrefab);
        currentLevelScope.gameObject.name = $"Level '{CurrentLevelName}'";
    }

    private void OnLevelWon()
    {
        
    }

    private void OnLevelLost()
    {
        
    }
}
