using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;

public class AudioPlayer : MonoBehaviour, IAudioPlayer
{
    [Header("Pool Setup")]
    [SerializeField] private Transform poolContainer;
    [SerializeField] private AudioSource sfxTemplatePrefab;
    
    [Header("Music Setup")]
    [SerializeField] private AudioSource musicSource;

    private ObjectPool<AudioSource> sfxPool;
    private readonly List<AudioSource> activeSfxSources = new List<AudioSource>();
    private SoundConfig currentMusicSoundConfig;
    private CancellationTokenSource fadeCTS;

    private bool muteMusic = false;

    private void Awake()
    {
        sfxPool = new ObjectPool<AudioSource>(
            createFunc: CreateNewSfxSource,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 30
        );
    }

    private void OnDestroy()
    {
        ResetFadeCTS();
    }

    private void Update()
    {
        for (int i = activeSfxSources.Count - 1; i >= 0; i--)
        {
            var source = activeSfxSources[i];
            if (!source.isPlaying)
            {
                activeSfxSources.RemoveAt(i);
                sfxPool.Release(source);
            }
        }
    }

    public void PlaySfx(SoundConfig config)
    {
        if (config == null) return;

        AudioSource source = sfxPool.Get();
        config.ApplyToSource(source);
        source.Play();
        
        activeSfxSources.Add(source);
    }

    public void PlayMusic(SoundConfig config, bool loop = true)
    {
        if (config == null) return;
        
        ResetFadeCTS();
        currentMusicSoundConfig = config;

        musicSource.Stop();
        config.ApplyToSource(musicSource);
        musicSource.loop = loop;
        musicSource.Play();

        if(muteMusic)
        {
            musicSource.volume = 0;
        }
    }

    public async UniTask StopMusic(float fadeDuration = 0)
    {
        ResetFadeCTS();

        if (fadeDuration <= 0f)
        {
            musicSource.Stop();
            return;
        }

        fadeCTS = new CancellationTokenSource();
        
        try
        {
            await FadeOutAndStopMusicAsync(fadeDuration, fadeCTS.Token);
        }
        catch (System.OperationCanceledException)
        {}
    }

    public void ToggleMusicMute()
    {
        muteMusic = !muteMusic;
        if(muteMusic)
        {
            musicSource.volume = 0;
        }
        else
        {
            musicSource.volume = currentMusicSoundConfig.Volume;
        }
    }

    private async UniTask FadeOutAndStopMusicAsync(float duration, CancellationToken token)
    {
        if(currentMusicSoundConfig == null) return;

        float startVolume = musicSource.volume;
        float elapsed = 0f;

        try
        {
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                
                await UniTask.Yield(PlayerLoopTiming.Update, token); 
            }
            musicSource.Stop();
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                if (currentMusicSoundConfig != null)
                {
                    currentMusicSoundConfig.ApplyToSource(musicSource);
                }
                musicSource.Stop(); 
            }
        }
    }

    private void ResetFadeCTS()
    {
        if (fadeCTS != null)
        {
            fadeCTS.Cancel();
            fadeCTS.Dispose();
            fadeCTS = null;
        }
    }

    private AudioSource CreateNewSfxSource()
    {
        return Instantiate(sfxTemplatePrefab, poolContainer);
    }

    private void OnTakeFromPool(AudioSource source)
    {
        source.gameObject.SetActive(true);
    }

    private void OnReturnedToPool(AudioSource source)
    {
        source.Stop();
        source.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(AudioSource source)
    {
        if (source != null)
        {
            Destroy(source.gameObject);
        }
    }
}