using Cysharp.Threading.Tasks;

public interface IAudioPlayer
{
    void PlaySfx(SoundConfig config);
    void PlayMusic(SoundConfig config, bool loop = true);
    UniTask StopMusic(float fadeDuration);
}