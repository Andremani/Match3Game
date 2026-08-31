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

    [SerializeField] private PieceViewMappingConfig pieceViewMapping;

    string maxTurnsText;
    string goalAmountText;
    int goalAmount;
    int piecesCollected;
    string winPieceKey;

    [Inject] private GlobalEventBroker globalEventBroker;
    [Inject] private GameManager gameManager;
    [Inject] private GameConfiguration gameConfiguration;

    public void Start()
    {
        levelUIRoot.SetActive(false);

        gameManager.LevelStarted += OnLevelStarted;
        globalEventBroker.PiecesCollected += OnPiecesCollected;
        globalEventBroker.TurnMade += OnTurnMade;
    }

    private void OnDestroy()
    {
        gameManager.LevelStarted -= OnLevelStarted;
        globalEventBroker.PiecesCollected -= OnPiecesCollected;
        globalEventBroker.TurnMade -= OnTurnMade;
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
