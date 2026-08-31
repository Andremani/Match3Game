using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AudioPlayer : MonoBehaviour, IAudioPlayer
{
    [Header("Pool Setup")]
    [SerializeField] private Transform poolContainer;
    [SerializeField] private AudioSource sfxTemplatePrefab;
    
    [Header("Music Setup")]
    [SerializeField] private AudioSource musicSource;

    private ObjectPool<AudioSource> _sfxPool;
    private readonly List<AudioSource> _activeSfxSources = new List<AudioSource>();

    private void Awake()
    {
        _sfxPool = new ObjectPool<AudioSource>(
            createFunc: CreateNewSfxSource,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 30
        );
    }

    private void Update()
    {
        for (int i = _activeSfxSources.Count - 1; i >= 0; i--)
        {
            var source = _activeSfxSources[i];
            if (!source.isPlaying)
            {
                _activeSfxSources.RemoveAt(i);
                _sfxPool.Release(source);
            }
        }
    }

    public void PlaySfx(SoundConfig config)
    {
        if (config == null) return;

        AudioSource source = _sfxPool.Get();
        config.ApplyToSource(source);
        source.Play();
        
        _activeSfxSources.Add(source);
    }

    public void PlayMusic(SoundConfig config, bool loop = true)
    {
        if (config == null) return;

        musicSource.Stop();
        config.ApplyToSource(musicSource);
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
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