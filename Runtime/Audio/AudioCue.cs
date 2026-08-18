using UnityEngine;
using UnityEngine.Audio;

namespace XrealBase.Audio
{
    [CreateAssetMenu(menuName = "XrealBase/Audio/AudioCue", fileName = "AudioCue_New")]
    public class AudioCue : ScriptableObject
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop;

        [Tooltip("エフェクトをかけたい場合は AudioMixerGroup を指定。null = デフォルト出力")]
        public AudioMixerGroup mixerGroup;

        [Header("BGM フェード設定")]
        [Tooltip("BGMクロスフェード時のフェードイン時間（秒）")]
        public float fadeInDuration = 1f;
        [Tooltip("次のBGMに切り替わる際のフェードアウト時間（秒）")]
        public float fadeOutDuration = 0.5f;
    }
}
