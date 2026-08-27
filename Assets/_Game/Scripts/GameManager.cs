using Configuration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class GameManager : MonoBehaviour
{
    private InputAction startAction;
    private LevelLifetimeScope levelLifetimeScopePrefab;
    private LevelLifetimeScope currentLevelScope;
    private ConfigReader configReader;

    public string CurrentLevelName { get; private set; } = string.Empty;

    [Inject]
    public void Construct(LevelLifetimeScope levelLifetimeScopePrefab, ConfigReader configReader)
    {
        this.levelLifetimeScopePrefab = levelLifetimeScopePrefab;
        this.configReader = configReader;
    }

    private void Start()
    {
        startAction = InputSystem.actions.FindAction("Submit");
        startAction.canceled += OnStartGameAction;
    }

    private void OnStartGameAction(InputAction.CallbackContext callback) => StartFirstLevelAsync().Forget();

    private async UniTaskVoid StartFirstLevelAsync()
    {
        if (currentLevelScope != null) return;

        LevelsListConfig levelsList = await configReader.LoadLevelsListAsync();
        await StartLevelAsync(levelsList.LevelNames[0]);
    }

    private async UniTask StartLevelAsync(string levelName)
    {
        CurrentLevelName = levelName;

        PieceMappingsConfig pieceMappings = await configReader.LoadPieceMappingsAsync();
        LevelConfig levelConfig = await configReader.LoadLevelConfigAsync(levelName);

        using (LifetimeScope.Enqueue(builder =>
        {
            builder.RegisterInstance(pieceMappings);
            builder.RegisterInstance(levelConfig);
        }))
        currentLevelScope = Instantiate(levelLifetimeScopePrefab);
        currentLevelScope.gameObject.name = $"Level '{levelName}'";
    }
}
