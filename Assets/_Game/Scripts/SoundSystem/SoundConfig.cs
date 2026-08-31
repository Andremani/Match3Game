using UnityEngine;

[System.Serializable]
public class SoundConfig
{
    [SerializeField] private AudioClip[] clips;
    
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;
    [Range(0.5f, 1.5f)] [SerializeField] private float minPitch = 0.95f;
    [Range(0.5f, 1.5f)] [SerializeField] private float maxPitch = 1.05f;

    public float Volume => volume;
    public void ApplyToSource(AudioSource source)
    {
        if (clips == null || clips.Length == 0) return;
        
        source.clip = clips[Random.Range(0, clips.Length)];
        source.volume = volume;
        source.pitch = Random.Range(minPitch, maxPitch);
    }
}