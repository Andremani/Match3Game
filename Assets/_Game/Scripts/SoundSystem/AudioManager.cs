using System.Collections.Generic;
using Configuration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class AudioManager : MonoBehaviour
{
    private const float DefaultMusicFadeOutDuration = 0.5f;

    [Inject] private GlobalEventBroker globalEventBroker;
    [Inject] private AudioDatabase audioDatabase;
    [Inject] private AudioPlayer audioPlayer;

    public void Start()
    {
        globalEventBroker.LevelStarted += OnLevelStarted;
        globalEventBroker.LevelWon += OnLevelWon;
        globalEventBroker.LevelLost += OnLevelLost;
        globalEventBroker.MatchSucceeded += OnMatchSucceeded;
        globalEventBroker.MatchFailed += OnMatchFailed;
        globalEventBroker.TurnMade += OnTurnMade;
    }

    private void OnDestroy()
    {
        globalEventBroker.LevelStarted -= OnLevelStarted;
        globalEventBroker.LevelWon -= OnLevelWon;
        globalEventBroker.LevelLost -= OnLevelLost;
        globalEventBroker.MatchSucceeded -= OnMatchSucceeded;
        globalEventBroker.MatchFailed -= OnMatchFailed;
        globalEventBroker.TurnMade -= OnTurnMade;
    }

    private void OnLevelStarted(int levelNumber, LevelConfig levelConfig)
    {
        SoundConfig music1 = audioDatabase.GetSound("music1");
        audioPlayer.PlayMusic(music1);
    }

    private void OnLevelWon()
    {
        LevelWonMusicAsync().Forget();
    }

    private async UniTaskVoid LevelWonMusicAsync()
    {
        await audioPlayer.StopMusic(DefaultMusicFadeOutDuration);
        SoundConfig winSound = audioDatabase.GetSound("win");
        audioPlayer.PlaySfx(winSound);
    }

    private void OnLevelLost()
    {
        SoundConfig failSound = audioDatabase.GetSound("fail");
        audioPlayer.PlaySfx(failSound);
    }

    private void OnMatchSucceeded(IReadOnlyList<MatchGroup> matchGroups)
    {
        SoundConfig matchSound = audioDatabase.GetSound("match_pop");
        audioPlayer.PlaySfx(matchSound);
    }

    private void OnMatchFailed()
    {
        SoundConfig wrongMatchSound = audioDatabase.GetSound("wrong_match");
        audioPlayer.PlaySfx(wrongMatchSound);
    }

    private void OnTurnMade(int turnNumber)
    {
        SoundConfig moveSound = audioDatabase.GetSound("move");
        audioPlayer.PlaySfx(moveSound);
    }
}
