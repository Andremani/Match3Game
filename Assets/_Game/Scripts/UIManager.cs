using Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Pieces;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject levelUIRoot;
    [SerializeField] TextMeshProUGUI levelTitle;
    [SerializeField] TextMeshProUGUI goalText;
    [SerializeField] Image goalIcon;
    [SerializeField] TextMeshProUGUI turnsText;
    [SerializeField] Button restartButton;

    [SerializeField] GameObject titleScreenRoot;
    [SerializeField] Button startButton;

    [SerializeField] GameObject winViewRoot;
    [SerializeField] Button winRestartButton;
    [SerializeField] Button winNextLevelButton;

    [SerializeField] GameObject loseViewRoot;
    [SerializeField] Button loseRestartButton;

    [SerializeField] GameObject winGameScreenRoot;

    [SerializeField] private PieceViewMappingConfig pieceViewMapping;

    string maxTurnsText;
    string goalAmountText;
    int goalAmount;
    int piecesCollected;
    string winPieceKey;

    [Inject] private GlobalEventBroker globalEventBroker;
    [Inject] private GameConfiguration gameConfiguration;

    public void Start()
    {
        levelUIRoot.SetActive(false);
        winViewRoot.SetActive(false);
        loseViewRoot.SetActive(false);
        titleScreenRoot.SetActive(true);
        winGameScreenRoot.SetActive(false);

        startButton.onClick.AddListener(OnStartButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        winRestartButton.onClick.AddListener(OnRestartButtonClicked);
        winNextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        loseRestartButton.onClick.AddListener(OnRestartButtonClicked);

        globalEventBroker.LevelStarted += OnLevelStarted;
        globalEventBroker.PiecesCollected += OnPiecesCollected;
        globalEventBroker.TurnMade += OnTurnMade;
        globalEventBroker.LevelWon += OnLevelWon;
        globalEventBroker.LevelLost += OnLevelLost;
        globalEventBroker.GameWin += OnGameWin;
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveListener(OnStartButtonClicked);
        restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        winRestartButton.onClick.RemoveListener(OnRestartButtonClicked);
        winNextLevelButton.onClick.RemoveListener(OnNextLevelButtonClicked);
        loseRestartButton.onClick.RemoveListener(OnRestartButtonClicked);

        globalEventBroker.LevelStarted -= OnLevelStarted;
        globalEventBroker.PiecesCollected -= OnPiecesCollected;
        globalEventBroker.TurnMade -= OnTurnMade;
        globalEventBroker.LevelWon -= OnLevelWon;
        globalEventBroker.LevelLost -= OnLevelLost;
    }

    private void OnStartButtonClicked()
    {
        titleScreenRoot.SetActive(false);
        globalEventBroker.RaiseGameStartRequest();
    }

    private void OnRestartButtonClicked()
    {
        winViewRoot.SetActive(false);
        loseViewRoot.SetActive(false);
        globalEventBroker.RaiseRestartRequest();
    }

    private void OnNextLevelButtonClicked()
    {
        winViewRoot.SetActive(false);
        globalEventBroker.RaiseNextLevelRequest();
    }

    private void OnLevelWon()
    {
        winViewRoot.SetActive(true);
    }

    private void OnLevelLost()
    {
        loseViewRoot.SetActive(true);
    }

    private void OnGameWin()
    {
        winGameScreenRoot.SetActive(true);
    }

    private void OnLevelStarted(int levelNumber, LevelConfig levelConfig)
    {
        levelUIRoot.SetActive(true);

        levelTitle.text = $"Level {levelNumber}";

        goalAmountText = levelConfig == null ? "?" : $"{levelConfig.WinPieceAmount}";
        goalAmount = levelConfig.WinPieceAmount;

        char winPieceChar = levelConfig.WinPieceType;
        winPieceKey = gameConfiguration.PieceMappingsConfig.PieceKeysBySymbol[winPieceChar];
        Sprite winPieceSprite = pieceViewMapping.GetSprite(winPieceKey);
        goalIcon.sprite = winPieceSprite;

        piecesCollected = 0;
        OnPiecesCollected(winPieceKey, 0);

        maxTurnsText = levelConfig == null ? "?" : $"{levelConfig.MaxTurns}";

        OnTurnMade(0);
    }

    private void OnTurnMade(int turnNumber)
    {
        turnsText.text = $"{turnNumber}/{maxTurnsText}";
    }

    private void OnPiecesCollected(string pieceKey, int count)
    {
        if(winPieceKey != pieceKey) return;
        piecesCollected += count; //I know, this is not a great practice to count them here
        piecesCollected = Mathf.Clamp(piecesCollected, 0, goalAmount);
        goalText.text = $"{piecesCollected}/{goalAmountText}";
    }
}
