using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrealBase.Audio
{
    /// <summary>
    /// BGM / ボイスの再生を一元管理するシングルトン。
    /// AudioCue に AudioMixerGroup を設定するとクリップごとにエフェクトが変わる。
    /// </summary>
    public class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }

        [SerializeField] AudioSource m_BgmSource;
        [SerializeField] AudioSource m_VoiceSource;
        [SerializeField] AudioSource m_AmbientSource;

        Coroutine m_BgmCoroutine;
        Coroutine m_AmbientCoroutine;
        Coroutine m_VoiceSequenceCoroutine;

        /// <summary>ボイスシーケンス（または単発ボイス）が再生中かどうか。</summary>
        public bool IsVoicePlaying => m_VoiceSequenceCoroutine != null || (m_VoiceSource != null && m_VoiceSource.isPlaying);

        /// <summary>ボイス再生に使う AudioSource。リップシンクなど外部から波形を参照する用途向け。</summary>
        public AudioSource VoiceSource => m_VoiceSource;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (m_BgmSource     == null) m_BgmSource     = gameObject.AddComponent<AudioSource>();
            if (m_VoiceSource   == null) m_VoiceSource   = gameObject.AddComponent<AudioSource>();
            if (m_AmbientSource == null) m_AmbientSource = gameObject.AddComponent<AudioSource>();
            m_BgmSource.playOnAwake     = false;
            m_VoiceSource.playOnAwake   = false;
            m_AmbientSource.playOnAwake = false;
        }

        // ──── BGM ────

        public void PlayBGM(AudioCue cue)
        {
            if (cue == null || cue.clip == null) return;
            if (m_BgmCoroutine != null) StopCoroutine(m_BgmCoroutine);
            m_BgmCoroutine = StartCoroutine(CrossfadeBGM(cue));
        }

        public void StopBGM(float fadeDuration = 1f)
        {
            if (m_BgmCoroutine != null) StopCoroutine(m_BgmCoroutine);
            m_BgmCoroutine = StartCoroutine(FadeOut(m_BgmSource, fadeDuration));
        }

        public void StopBGM() => StopBGM(1f);

        // ──── アンビエント ────

        public void PlayAmbient(AudioCue cue)
        {
            if (cue == null || cue.clip == null) return;
            if (m_AmbientCoroutine != null) StopCoroutine(m_AmbientCoroutine);
            m_AmbientCoroutine = StartCoroutine(CrossfadeAmbient(cue));
        }

        public void StopAmbient(float fadeDuration = 1f)
        {
            if (m_AmbientCoroutine != null) StopCoroutine(m_AmbientCoroutine);
            m_AmbientCoroutine = StartCoroutine(FadeOut(m_AmbientSource, fadeDuration));
        }

        public void StopAmbient() => StopAmbient(1f);

        // ──── ボイス（単発） ────

        public void PlayVoice(AudioCue cue)
        {
            if (cue == null || cue.clip == null) return;
            StopVoiceSequence();
            PlayVoiceImmediate(cue);
        }

        public void StopVoice()
        {
            StopVoiceSequence();
            m_VoiceSource.Stop();
        }

        // ──── ボイス（順番再生） ────

        public void PlayVoiceSequence(IList<VoiceEntry> entries)
        {
            StopVoiceSequence();
            if (entries == null || entries.Count == 0) return;
            m_VoiceSequenceCoroutine = StartCoroutine(VoiceSequenceCoroutine(entries));
        }

        // ──── 内部 ────

        void StopVoiceSequence()
        {
            if (m_VoiceSequenceCoroutine != null)
            {
                StopCoroutine(m_VoiceSequenceCoroutine);
                m_VoiceSequenceCoroutine = null;
            }
        }

        void ApplyCueToSource(AudioSource src, AudioCue cue)
        {
            src.clip                  = cue.clip;
            src.volume                = cue.volume;
            src.loop                  = cue.loop;
            src.outputAudioMixerGroup = cue.mixerGroup;
        }

        void PlayVoiceImmediate(AudioCue cue)
        {
            m_VoiceSource.Stop();
            ApplyCueToSource(m_VoiceSource, cue);
            m_VoiceSource.Play();
        }

        IEnumerator VoiceSequenceCoroutine(IList<VoiceEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (entry?.cue == null || entry.cue.clip == null) continue;

                if (entry.delayAfterPrev > 0f)
                    yield return new WaitForSeconds(entry.delayAfterPrev);

                PlayVoiceImmediate(entry.cue);
                yield return new WaitForSeconds(entry.cue.clip.length);
            }
            m_VoiceSequenceCoroutine = null;
        }

        IEnumerator CrossfadeAmbient(AudioCue cue)
        {
            if (m_AmbientSource.isPlaying)
                yield return FadeOut(m_AmbientSource, cue.fadeOutDuration);

            ApplyCueToSource(m_AmbientSource, cue);
            m_AmbientSource.volume = 0f;
            m_AmbientSource.Play();

            if (cue.fadeInDuration > 0f)
            {
                var elapsed = 0f;
                while (elapsed < cue.fadeInDuration)
                {
                    elapsed += Time.deltaTime;
                    m_AmbientSource.volume = Mathf.Lerp(0f, cue.volume, elapsed / cue.fadeInDuration);
                    yield return null;
                }
            }

            m_AmbientSource.volume = cue.volume;
        }

        IEnumerator CrossfadeBGM(AudioCue cue)
        {
            if (m_BgmSource.isPlaying)
                yield return FadeOut(m_BgmSource, cue.fadeOutDuration);

            ApplyCueToSource(m_BgmSource, cue);
            m_BgmSource.volume = 0f;
            m_BgmSource.Play();

            if (cue.fadeInDuration > 0f)
            {
                var elapsed = 0f;
                while (elapsed < cue.fadeInDuration)
                {
                    elapsed += Time.deltaTime;
                    m_BgmSource.volume = Mathf.Lerp(0f, cue.volume, elapsed / cue.fadeInDuration);
                    yield return null;
                }
            }

            m_BgmSource.volume = cue.volume;
        }

        IEnumerator FadeOut(AudioSource src, float duration)
        {
            var start   = src.volume;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed    += Time.deltaTime;
                src.volume  = Mathf.Lerp(start, 0f, elapsed / duration);
                yield return null;
            }
            src.Stop();
            src.volume = start;
        }
    }
}
