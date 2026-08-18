using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace XrealBase.AR
{
    /// <summary>
    /// 実機デバッグ用: マーカートラッキングの状態を TMP テキストで画面に表示する。
    /// Canvas と TMP_Text は自分で用意してアサインすること。
    /// 不要になったら GameObject ごと削除すること。
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class ARMarkerDebugHUD : MonoBehaviour
    {
        [Header("TMP Text (インスペクターでアサイン)")]
        [SerializeField] TMP_Text m_Text;

        [Header("表示設定")]
        [SerializeField] float m_UpdateInterval = 0.5f;

        ARTrackedImageManager m_Manager;
        XRReferenceImageLibrary m_Library;
        float m_Timer;

        readonly List<string> m_EventLog = new();
        readonly StringBuilder m_Sb = new();

        void Awake()
        {
            m_Manager = GetComponent<ARTrackedImageManager>();

            if (m_Text == null)
                Debug.LogWarning("[ARMarkerDebugHUD] TMP_Text がアサインされていません。インスペクターで設定してください。");
        }

        void OnEnable()
        {
            m_Manager.trackedImagesChanged += OnTrackedImagesChanged;
            // referenceLibrary はビルド前は XRReferenceImageLibrary、実機では RuntimeReferenceImageLibrary
            m_Library = m_Manager.referenceLibrary as XRReferenceImageLibrary;
            if (m_Library == null)
                Debug.Log($"[ARMarkerDebugHUD] runtime library type: {m_Manager.referenceLibrary?.GetType().Name ?? "null"}");
        }

        void OnDisable()
        {
            m_Manager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs e)
        {
            foreach (var img in e.added)
                Log($"[ADDED] {img.referenceImage.name} state={img.trackingState}");
            foreach (var img in e.updated)
                Log($"[UPDATED] {img.referenceImage.name} state={img.trackingState}");
            foreach (var img in e.removed)
                Log($"[REMOVED] {img.referenceImage.name}");
        }

        void Log(string msg)
        {
            m_EventLog.Add($"{Time.time:F1}s {msg}");
            if (m_EventLog.Count > 8) m_EventLog.RemoveAt(0);
        }

        void Update()
        {
            m_Timer += Time.deltaTime;
            if (m_Timer < m_UpdateInterval) return;
            m_Timer = 0f;
            RefreshText();
        }

        void RefreshText()
        {
            if (m_Text == null) return;

            m_Sb.Clear();
            m_Sb.AppendLine("=== AR Marker Debug ===");

            // ── Library ──────────────────────────────
            var runtimeLib = m_Manager.referenceLibrary;
            m_Sb.Append("Library: ");
            if (runtimeLib == null)
            {
                m_Sb.AppendLine("<color=red>NULL (ARTrackedImageManager に未アサイン)</color>");
            }
            else
            {
                // 実機では RuntimeReferenceImageLibrary (XREALImageDatabase) になる
                m_Sb.AppendLine($"{runtimeLib.GetType().Name}  count={runtimeLib.count}");

                // エディター時のみ XRReferenceImageLibrary にキャストできる
                if (m_Library != null)
                {
                    bool hasXrealData = m_Library.dataStore.TryGetValue("com.xreal.xr", out var xrealBytes)
                                        && xrealBytes != null && xrealBytes.Length > 0;
                    m_Sb.AppendLine(hasXrealData
                        ? $"  XREAL DB: <color=green>OK ({xrealBytes.Length} bytes)</color>"
                        : "  XREAL DB: <color=red>空！Android ビルドで再生成が必要</color>");
                }
                else
                {
                    // 実機: count=0 なら DB が空でロード失敗の可能性大
                    if (runtimeLib.count == 0)
                        m_Sb.AppendLine("  <color=red>count=0: XREAL DB が空か初期化失敗の疑い</color>");
                    else
                        m_Sb.AppendLine($"  <color=green>count={runtimeLib.count} (DB ロード成功の可能性あり)</color>");
                }
            }

            // ── ARTrackedImageManager ─────────────────
            m_Sb.AppendLine();
            m_Sb.Append("TrackedImageManager: ");
            if (m_Manager == null)
            {
                m_Sb.AppendLine("<color=red>NULL</color>");
            }
            else
            {
                string enStr = m_Manager.isActiveAndEnabled
                    ? "<color=green>有効</color>"
                    : "<color=red>無効</color>";
                m_Sb.AppendLine($"{enStr}  maxMoving={m_Manager.requestedMaxNumberOfMovingImages}");
            }

            // ── XRImageTrackingSubsystem ──────────────
            var subsystems = new List<XRImageTrackingSubsystem>();
            SubsystemManager.GetSubsystems(subsystems);
            m_Sb.Append("Subsystem: ");
            if (subsystems.Count == 0)
            {
                m_Sb.AppendLine("<color=red>未登録 (XRLoader 確認)</color>");
            }
            else
            {
                var sub = subsystems[0];
                string runStr = sub.running
                    ? "<color=green>running</color>"
                    : "<color=yellow>stopped</color>";
                m_Sb.AppendLine(runStr);
            }

            // ── 現在トラッキング中 ────────────────────
            m_Sb.AppendLine();
            m_Sb.Append("Tracking now: ");
            int trackingCount = 0;
            foreach (var img in m_Manager.trackables)
                if (img.trackingState == TrackingState.Tracking) trackingCount++;

            if (trackingCount == 0)
            {
                m_Sb.AppendLine("<color=yellow>なし</color>");
            }
            else
            {
                m_Sb.AppendLine($"<color=green>{trackingCount} 件</color>");
                foreach (var img in m_Manager.trackables)
                    m_Sb.AppendLine($"  {img.referenceImage.name} [{img.trackingState}]");
            }

            // ── イベントログ ──────────────────────────
            m_Sb.AppendLine();
            m_Sb.AppendLine("Events (最新8件):");
            if (m_EventLog.Count == 0)
                m_Sb.AppendLine("  (なし)");
            else
                foreach (var e in m_EventLog)
                    m_Sb.AppendLine($"  {e}");

            m_Text.text = m_Sb.ToString();
        }
    }
}
