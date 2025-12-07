using UnityEngine;

public class MusicAudioHandler : AbstractAudioHandler
{
    private float lastPlaybackTime = 0f;

    public override void PlayClip(AudioClip clip, bool loop = true, float volume = 0.5f)
    {
        if (clip == null) return;

        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = volume;
        audioSource.time = lastPlaybackTime; // resume from last time if paused
        audioSource.Play();
    }

    public void Pause()
    {
        if (audioSource.isPlaying)
        {
            lastPlaybackTime = audioSource.time;
            audioSource.Pause();
        }
    }

    public void Resume()
    {
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.time = lastPlaybackTime;
            audioSource.Play();
        }
    }

    public void Stop()
    {
        lastPlaybackTime = 0f;
        audioSource.Stop();
    }
}
