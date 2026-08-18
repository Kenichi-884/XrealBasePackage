using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XrealBase.Audio;

namespace XrealBase.Scene
{
    /// <summary>
    /// 体験全体のシーン進行を管理するディレクター。
    /// Unity シーンは切り替えず、内部ステートだけを進める。
    ///
    /// 使い方:
    ///   1. EventSceneConfig (ScriptableObject) を各シーン分作成してアサイン
    ///   2. 各 SceneEntry の onEnter / onExit で GameObject の表示切替などを配線
    ///   3. コード / UnityEvent から AdvanceScene() or GoToScene("id") を呼ぶ
    ///   4. autoAdvanceOnVoicesComplete を ON にするとボイス終了後に自動で次へ進む
    /// </summary>
    public class EventSceneDirector : MonoBehaviour
    {
        [System.Serializable]
        public class SceneEntry
        {
            public EventSceneConfig config;

            [Tooltip("このシーンに入ったときに発火 (GameObject表示、エフェクト開始など)")]
            public UnityEvent onEnter;

            [Tooltip("このシーンを出るときに発火 (GameObject非表示など)")]
            public UnityEvent onExit;

            [Tooltip("ON: ボイスシーケンスが全て終わったら自動で次のシーンへ進む")]
            public bool autoAdvanceOnVoicesComplete;
        }

        [SerializeField] List<SceneEntry> m_Scenes = new();
        [SerializeField] bool             m_AutoStartFirstScene = true;

        public int    CurrentIndex   { get; private set; } = -1;
        public string CurrentSceneId => CurrentIndex >= 0 && m_Scenes[CurrentIndex].config != null
                                        ? m_Scenes[CurrentIndex].config.sceneId : null;

        [Header("通知")]
        public UnityEvent<string> onSceneEntered; // 引数: sceneId

        Coroutine m_AudioCoroutine;

        // ──── ライフサイクル ────

        void Start()
        {
            if (m_AutoStartFirstScene && m_Scenes.Count > 0)
                GoToScene(0);
        }

        // ──── 公開API ────

        /// <summary>次のシーンへ進む。最後のシーンで呼んでも何もしない。</summary>
        public void AdvanceScene() => GoToScene(CurrentIndex + 1);

        /// <summary>インデックス指定でシーンへ移動。</summary>
        public void GoToScene(int index)
        {
            if (index < 0 || index >= m_Scenes.Count) return;

            if (CurrentIndex >= 0)
                m_Scenes[CurrentIndex].onExit?.Invoke();

            CurrentIndex = index;
            var entry = m_Scenes[CurrentIndex];
            entry.onEnter?.Invoke();

            if (m_AudioCoroutine != null) StopCoroutine(m_AudioCoroutine);
            if (entry.config != null)
                m_AudioCoroutine = StartCoroutine(PlaySceneAudio(entry.config, entry.autoAdvanceOnVoicesComplete));

            onSceneEntered?.Invoke(entry.config?.sceneId);
            Debug.Log($"[SceneDirector] → {entry.config?.sceneId ?? $"Scene{index}"}");
        }

        /// <summary>ID指定でシーンへ移動。</summary>
        public void GoToScene(string id)
        {
            var idx = m_Scenes.FindIndex(s => s.config != null && s.config.sceneId == id);
            if (idx >= 0) GoToScene(idx);
            else Debug.LogWarning($"[SceneDirector] sceneId '{id}' が見つかりません。");
        }

        // ──── 内部 ────

        IEnumerator PlaySceneAudio(EventSceneConfig cfg, bool autoAdvance)
        {
            if (AudioDirector.Instance == null) yield break;

            if (cfg.bgm != null)
            {
                if (cfg.bgmDelay > 0f) yield return new WaitForSeconds(cfg.bgmDelay);
                AudioDirector.Instance.PlayBGM(cfg.bgm);
            }

            if (cfg.ambient != null)
            {
                if (cfg.ambientDelay > 0f) yield return new WaitForSeconds(cfg.ambientDelay);
                AudioDirector.Instance.PlayAmbient(cfg.ambient);
            }

            if (cfg.voices != null && cfg.voices.Count > 0)
            {
                // マーカーボイス等が既に再生中の場合はシーンボイスで上書きしない
                if (!AudioDirector.Instance.IsVoicePlaying)
                    AudioDirector.Instance.PlayVoiceSequence(cfg.voices);

                if (autoAdvance)
                {
                    // ボイスが開始されるまで1フレーム待つ
                    yield return null;
                    yield return new WaitUntil(() =>
                        AudioDirector.Instance == null || !AudioDirector.Instance.IsVoicePlaying);
                    AdvanceScene();
                }
            }
        }
    }
}
