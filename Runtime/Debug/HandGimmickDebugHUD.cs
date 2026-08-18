using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Hands;
using XrealBase.Interaction;

namespace XrealBase.Debug
{
    /// <summary>
    /// Device debug HUD: displays hand tracking state, palm direction, pinch distance,
    /// and Window Gimmick progress on a TMP_Text component.
    /// Assign Canvas + TMP_Text in the Inspector.
    /// Remove this GameObject when no longer needed.
    /// </summary>
    public class HandGimmickDebugHUD : MonoBehaviour
    {
        [Header("TMP Text (assign in Inspector)")]
        [SerializeField] TMP_Text m_Text;

        [Header("Gimmick References")]
        [SerializeField] PalmSummonWindow m_PalmSummon;
        [SerializeField] PinchOpenDoors   m_PinchDoors;

        [Header("Settings")]
        [SerializeField] float m_UpdateInterval = 0.1f;

        XRHandSubsystem m_Subsystem;
        float m_Timer;
        readonly StringBuilder m_Sb = new();

        void Awake()
        {
            if (m_Text == null)
                UnityEngine.Debug.LogWarning("[HandGimmickDebugHUD] TMP_Text is not assigned.");
        }

        XRHandSubsystem GetSubsystem()
        {
            if (m_Subsystem != null && m_Subsystem.running) return m_Subsystem;
            var list = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(list);
            m_Subsystem = list.Count > 0 ? list[0] : null;
            return m_Subsystem;
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

            m_Sb.AppendLine("=== Hand Gimmick Debug ===");

            // ── Gimmick state ──────────────────────────────
            m_Sb.AppendLine();
            m_Sb.AppendLine("[Gimmick State]");

            if (m_PalmSummon != null)
            {
                if (!m_PalmSummon.Summoned)
                {
                    m_Sb.AppendLine("  Window: <color=yellow>Waiting - face palm up to summon</color>");
                    float holdPct = m_PalmSummon.HoldSeconds > 0f
                        ? m_PalmSummon.HoldTimer / m_PalmSummon.HoldSeconds
                        : 0f;
                    m_Sb.AppendLine($"  Hold: {m_PalmSummon.HoldTimer:F2}s / {m_PalmSummon.HoldSeconds:F1}s  [{ProgressBar(holdPct, 10)}]");
                }
                else
                {
                    m_Sb.AppendLine("  Window: <color=green>Summoned</color>");
                }
            }
            else
            {
                m_Sb.AppendLine("  PalmSummonWindow: <color=red>Not assigned</color>");
            }

            if (m_PinchDoors != null)
            {
                if (m_PalmSummon == null || !m_PalmSummon.Summoned)
                {
                    m_Sb.AppendLine("  Door: <color=#888888>Waiting for window summon</color>");
                }
                else if (!m_PinchDoors.Opening && !m_PinchDoors.Opened)
                {
                    float remaining = Mathf.Max(0f, m_PinchDoors.StartupDelay - m_PinchDoors.EnabledTimer);
                    if (remaining > 0f)
                        m_Sb.AppendLine($"  Door: <color=yellow>Startup delay {remaining:F1}s</color>");
                    else
                        m_Sb.AppendLine("  Door: <color=cyan>Waiting for pinch</color>");
                }
                else if (m_PinchDoors.Opening && !m_PinchDoors.Opened)
                {
                    m_Sb.AppendLine("  Door: <color=green>Opening...</color>");
                }
                else
                {
                    m_Sb.AppendLine("  Door: <color=green>Opened</color>");
                }
            }
            else
            {
                m_Sb.AppendLine("  PinchOpenDoors: <color=red>Not assigned</color>");
            }

            // ── Hand tracking ──────────────────────────────
            m_Sb.AppendLine();
            var subsystem = GetSubsystem();
            if (subsystem == null)
            {
                m_Sb.AppendLine("[Hand Tracking]");
                m_Sb.AppendLine("  <color=red>XRHandSubsystem not found</color>");
            }
            else
            {
                AppendHandInfo(subsystem.leftHand,  "Left  (L)");
                m_Sb.AppendLine();
                AppendHandInfo(subsystem.rightHand, "Right (R)");
            }

            m_Text.text = m_Sb.ToString();
        }

        void AppendHandInfo(XRHand hand, string label)
        {
            m_Sb.Append($"[{label}] ");
            if (!hand.isTracked)
            {
                m_Sb.AppendLine("<color=red>Not tracked</color>");
                return;
            }
            m_Sb.AppendLine("<color=green>Tracked</color>");

            // Palm direction
            var palmJoint = hand.GetJoint(XRHandJointID.Palm);
            if (palmJoint.TryGetPose(out var palmPose))
            {
                float dot       = Vector3.Dot(-palmPose.up, Vector3.up);
                float threshold = m_PalmSummon != null ? m_PalmSummon.PalmUpDot : 0.7f;
                bool  isUp      = dot >= threshold;
                string col      = isUp ? "green" : "white";
                string state    = isUp ? "Up!" : "Down";
                m_Sb.AppendLine($"  Palm dir : <color={col}>{state}  dot={dot:F2} (threshold {threshold:F2})</color>");
            }

            // Pinch distance
            var thumbJoint = hand.GetJoint(XRHandJointID.ThumbTip);
            var indexJoint = hand.GetJoint(XRHandJointID.IndexTip);
            if (thumbJoint.TryGetPose(out var thumb) && indexJoint.TryGetPose(out var index))
            {
                float dist = Vector3.Distance(thumb.position, index.position);

                // ドア用閾値
                float doorThreshold = m_PinchDoors != null ? m_PinchDoors.PinchThreshold : 0.025f;
                bool  isDoorPinch   = dist < doorThreshold;
                string doorCol      = isDoorPinch ? "green" : "white";
                string doorState    = isDoorPinch ? "Pinch!" : "Open";
                m_Sb.AppendLine($"  Pinch dist: <color={doorCol}>{doorState}  {dist * 100f:F1}cm (door {doorThreshold * 100f:F1}cm)</color>");
            }
        }

        static string ProgressBar(float t, int width)
        {
            int filled = Mathf.RoundToInt(Mathf.Clamp01(t) * width);
            return new string('|', filled) + new string('-', width - filled);
        }
    }
}
