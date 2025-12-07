using UnityEngine;

public abstract class AbstractAudioHandler : MonoBehaviour
{
    protected AudioSource audioSource;

    public virtual void Init(AudioSource src)
    {
        audioSource = src;
    }

    public abstract void PlayClip(AudioClip clip, bool loop = false, float volume = 1f);
}
