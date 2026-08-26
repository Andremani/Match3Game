using Configuration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class GameManager : MonoBehaviour
{
    private InputAction startAction;
    private GameLifetimeScope parentScope;
    private ConfigReader configReader;

    public string CurrentLevelName { get; private set; } = string.Empty;

    [Inject]
    public void Construct(GameLifetimeScope parentScope, ConfigReader configReader)
    {
        this.parentScope = parentScope;
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
        LevelsListConfig levelsList = await configReader.LoadLevelsListAsync();
        CurrentLevelName = levelsList.LevelNames[0];
        LevelLifetimeScope levelLifetimeScope = parentScope.CreateChildFromPrefab(parentScope.LevelLifetimeScopePrefab, builder => builder.RegisterInstance(CurrentLevelName));
        levelLifetimeScope.gameObject.name = $"Level '{CurrentLevelName}'";
        Debug.Log($"Level '{CurrentLevelName}' created");
    }
}
