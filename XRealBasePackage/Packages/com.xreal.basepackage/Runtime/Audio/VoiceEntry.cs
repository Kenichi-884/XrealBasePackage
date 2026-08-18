using System;

namespace XrealBase.Audio
{
    /// <summary>順番再生するボイス1エントリ。</summary>
    [Serializable]
    public class VoiceEntry
    {
        public AudioCue cue;
        [UnityEngine.Tooltip("前のクリップが終わってから再生するまでの待機秒数。先頭エントリはシーン開始からの遅延。")]
        public float delayAfterPrev;
    }
}
