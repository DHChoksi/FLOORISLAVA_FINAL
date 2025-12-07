using System.Collections.Generic;
using UnityEngine;
using static Constants.Constants;

[CreateAssetMenu(menuName = "IGR/Audio Library")]
public class AudioLibrarySO : ScriptableObject
{
    [System.Serializable]
    public class SFXClip
    {
        [SerializeField]
        private SFXType m_SFXType;
        [SerializeField]
        private AudioClip m_SFXAudioClip;

        public SFXType SFXType { get { return m_SFXType; } }
        public AudioClip SFXAudioClip { get { return m_SFXAudioClip; } }
    }

    [System.Serializable]
    public class BGMClip
    {
        [SerializeField]
        private BackgroundMusicType m_BGMusicType;
        [SerializeField]
        private AudioClip m_BGMusicClip;

        public BackgroundMusicType BGMusicType {  get { return m_BGMusicType; }}
        public AudioClip BGMusicClip {  get { return m_BGMusicClip; }}  
    }

    [SerializeField]
    private List<SFXClip> m_SfxClips;

    [SerializeField]
    private List<BGMClip> m_BgMusicClips;

    [SerializeField]
    public AudioClip GetSFX(SFXType type) =>
        m_SfxClips.Find(x => x.SFXType == type)?.SFXAudioClip;

    [SerializeField]
    public AudioClip GetBGM(BackgroundMusicType type) =>
        m_BgMusicClips.Find(x => x.BGMusicType == type)?.BGMusicClip;
}
