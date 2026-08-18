using System.Collections.Generic;
using UnityEngine;
using XrealBase.Audio;

namespace XrealBase.Scene
{
    /// <summary>
    /// シーン1つ分のオーディオ設定データ。ScriptableObject なので複数シーンで使い回せる。
    /// </summary>
    [CreateAssetMenu(menuName = "XrealBase/Scene/EventSceneConfig", fileName = "Scene_New")]
    public class EventSceneConfig : ScriptableObject
    {
        [Tooltip("シーンを識別するID（EventSceneDirector.GoToScene(id) で参照）")]
        public string sceneId;

        [Header("BGM")]
        [Tooltip("このシーンのBGM。null = BGM変更なし")]
        public AudioCue bgm;
        [Tooltip("BGM再生開始までの遅延（秒）")]
        public float bgmDelay;

        [Header("アンビエント")]
        [Tooltip("このシーンのアンビエントサウンド。null = 変更なし")]
        public AudioCue ambient;
        [Tooltip("アンビエント再生開始までの遅延（秒）")]
        public float ambientDelay;

        [Header("ボイス（上から順番に再生）")]
        [Tooltip("シーン開始時に順番に再生するボイスリスト。空 = 再生なし")]
        public List<VoiceEntry> voices = new();
    }
}
