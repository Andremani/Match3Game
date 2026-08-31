using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class AudioManager : MonoBehaviour
{
    [Inject] private GlobalEventBroker globalEventBroker;
    [Inject] private AudioDatabase audioDatabase;
    [Inject] private AudioPlayer audioPlayer;

    public void Start()
    {
        globalEventBroker.LevelWon += OnLevelWon;
        globalEventBroker.LevelLost += OnLevelLost;
        globalEventBroker.MatchSucceeded += OnMatchSucceeded;
        globalEventBroker.MatchSucceeded += OnMatchFailed;
        globalEventBroker.TurnMade += OnTurnMade;

        SoundConfig music1 = audioDatabase.GetSound("music1");
        audioPlayer.PlayMusic(music1);
    }

    private void OnDestroy()
    {
        globalEventBroker.LevelWon -= OnLevelWon;
        globalEventBroker.LevelLost -= OnLevelLost;
        globalEventBroker.MatchSucceeded -= OnMatchSucceeded;
        globalEventBroker.MatchSucceeded -= OnMatchFailed;
        globalEventBroker.TurnMade -= OnTurnMade;
    }

    private void OnLevelWon()
    {
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

    private void OnMatchFailed(IReadOnlyList<MatchGroup> matchGroups)
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
