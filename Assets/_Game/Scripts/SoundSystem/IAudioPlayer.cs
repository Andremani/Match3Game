public interface IAudioPlayer
{
    void PlaySfx(SoundConfig config);
    void PlayMusic(SoundConfig config, bool loop = true);
    void StopMusic();
}