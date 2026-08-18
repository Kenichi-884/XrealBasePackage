using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace XrealBase.AR
{
    /// <summary>
    /// IMarkerContentSource 実装のイベントを受け取り、シーン・オーディオシステムへ中継するブリッジ。
    /// 検出元コンポーネント本体は変更せず疎結合を維持する。
    ///
    /// 使い方:
    ///   IMarkerContentSource を実装したコンポーネントと同じ GameObject にアタッチ。
    ///   onMarkerFound / onAllRequiredMarkersFound を Inspector で配線。
    ///   (例) onAllRequiredMarkersFound → EventSceneDirector.AdvanceScene()
    /// </summary>
    public class MarkerEventRelay : MonoBehaviour
    {
        [System.Serializable] public class MarkerEvent : UnityEvent<string, GameObject> { }

        [Tooltip("これらが全て検出されると onAllRequiredMarkersFound を発火する")]
        [SerializeField] List<string> m_RequiredMarkers = new();

        [Header("Events")]
        public MarkerEvent onMarkerFound             = new();
        public UnityEvent  onAllRequiredMarkersFound = new();

        readonly HashSet<string> m_Found = new();
        IMarkerContentSource     m_Source;

        void Awake()
        {
            m_Source = GetComponent<IMarkerContentSource>();
            if (m_Source == null)
                Debug.LogWarning($"[MarkerEventRelay] 同じ GameObject に IMarkerContentSource を実装したコンポーネントが見つかりません。");
        }

        void OnEnable()  { if (m_Source != null) m_Source.OnContentShown.AddListener(HandleShown); }
        void OnDisable() { if (m_Source != null) m_Source.OnContentShown.RemoveListener(HandleShown); }

        void HandleShown(string markerName, GameObject instance)
        {
            onMarkerFound.Invoke(markerName, instance);

            if (m_RequiredMarkers.Count == 0 || !m_RequiredMarkers.Contains(markerName)) return;
            m_Found.Add(markerName);
            if (m_Found.IsSupersetOf(m_RequiredMarkers))
                onAllRequiredMarkersFound.Invoke();
        }

        /// <summary>
        /// エディター・開発ビルドでマーカー検出をシミュレートする。
        /// SceneDebugController や Inspector の ContextMenu から呼ぶ。
        /// </summary>
        public void SimulateMarkerFound(string markerName)
        {
            Debug.Log($"[MarkerRelay] シミュレート検出: {markerName}");
            HandleShown(markerName, null);
        }
    }
}
