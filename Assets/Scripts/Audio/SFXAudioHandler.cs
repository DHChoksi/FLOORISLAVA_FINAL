using UnityEngine;
using System.Collections.Generic;

public class SFXAudioHandler : AbstractAudioHandler
{
    [SerializeField] private int poolSize = 10;
    private Queue<AudioSource> audioSourcePool;

    public override void Init(AudioSource src)
    {
        base.Init(src);
        audioSourcePool = new Queue<AudioSource>();

        // Create pool of audio sources
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource pooledSource = new GameObject("SFX_Source_" + i).AddComponent<AudioSource>();
            pooledSource.transform.SetParent(this.transform);
            pooledSource.playOnAwake = false;
            audioSourcePool.Enqueue(pooledSource);
        }
    }

    public override void PlayClip(AudioClip clip, bool loop = false, float volume = 1f)
    {
        if (clip == null || audioSourcePool.Count == 0) return;

        AudioSource source = audioSourcePool.Dequeue();
        source.clip = clip;
        source.loop = false;
        source.volume = volume;
        source.Play();

        StartCoroutine(ReturnToPoolAfterPlaying(source));
    }

    private System.Collections.IEnumerator ReturnToPoolAfterPlaying(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        source.clip = null;
        audioSourcePool.Enqueue(source);
    }
}
