using System.Linq;
using System.Text;
using Configuration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

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
        if(currentLevelScope != null) return;

        LevelsListConfig levelsList = await configReader.LoadLevelsListAsync();
        CurrentLevelName = levelsList.LevelNames[0];

        currentLevelScope = Instantiate(levelLifetimeScopePrefab);
        currentLevelScope.gameObject.name = $"Level '{CurrentLevelName}'";
        Debug.Log($"Level '{CurrentLevelName}' created");

        TestLogging().Forget();
    }

    private async UniTask TestLogging()
    {
        LevelsListConfig levelsList = await configReader.LoadLevelsListAsync();
        CurrentLevelName = levelsList.LevelNames[0];
        PieceMappingsConfig pieceMappings = await configReader.LoadPieceMappingsAsync();
        LevelConfig levelConfig = await configReader.LoadLevelConfigAsync(CurrentLevelName);

        LogLoadedConfigs(levelsList, pieceMappings, levelConfig);
    }

    private static void LogLoadedConfigs(LevelsListConfig levelsList, PieceMappingsConfig pieceMappings, LevelConfig levelConfig)
    {
        Debug.Log($"LevelsList: {string.Join(", ", levelsList.LevelNames)}");

        string mappingLines = string.Join("\n", pieceMappings.PieceKeysBySymbol.Select(pair => $"{pair.Key}: {pair.Value}"));
        Debug.Log($"PieceMappings:\n{mappingLines}");

        Debug.Log(
            $"LevelConfig: HorizontalSize={levelConfig.HorizontalSize}, VerticalSize={levelConfig.VerticalSize}, " +
            $"MaxRegularPieceTypes={levelConfig.MaxRegularPieceTypes}, WinPieceType={levelConfig.WinPieceType}, " +
            $"WinPieceAmount={levelConfig.WinPieceAmount}, MaxTurns={levelConfig.MaxTurns}\n{FormatMap(levelConfig.Map)}");
    }

    private static string FormatMap(char[,] map)
    {
        int horizontalSize = map.GetLength(0);
        int verticalSize = map.GetLength(1);
        StringBuilder builder = new();

        for (int y = 0; y < verticalSize; y++)
        {
            for (int x = 0; x < horizontalSize; x++)
            {
                builder.Append(map[x, y]);
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }
}
