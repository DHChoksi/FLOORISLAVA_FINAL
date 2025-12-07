using UnityEngine;
using static Constants.Constants;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Library")]
    public AudioLibrarySO audioLibrary;

    [Header("Handlers")]
    private SFXAudioHandler sfxHandler;
    private MusicAudioHandler musicHandler;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else Instance = this;

        DontDestroyOnLoad(gameObject);
        InitHandlers();
    }

    private void InitHandlers()
    {
        // SFX Pooling Setup
        var sfxObj = new GameObject("SFX_Handler");
        sfxObj.transform.SetParent(transform);
        sfxHandler = sfxObj.AddComponent<SFXAudioHandler>();
        sfxHandler.Init(sfxObj.AddComponent<AudioSource>());

        // Music Setup
        var musicObj = new GameObject("Music_Handler");
        musicObj.transform.SetParent(transform);
        musicHandler = musicObj.AddComponent<MusicAudioHandler>();
        musicHandler.Init(musicObj.AddComponent<AudioSource>());

        PlayMusic(BackgroundMusicType.BackgroundMusic);
    }

    // Public APIs
    public void PlaySFX(SFXType type, float volume = 1f)
    {
        var clip = audioLibrary.GetSFX(type);
        sfxHandler.PlayClip(clip, false, volume);
    }

    public void PlayMusic(BackgroundMusicType type, float volume = 0.5f)
    {
        var clip = audioLibrary.GetBGM(type);
        musicHandler.PlayClip(clip, true, volume);
    }

    public void PauseMusic() => musicHandler.Pause();

    public void ResumeMusic() => musicHandler.Resume();

    public void StopMusic() => musicHandler.Stop();
}
