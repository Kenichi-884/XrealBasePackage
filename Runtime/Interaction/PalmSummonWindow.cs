using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands;
using XrealBase.Effects;

namespace XrealBase.Interaction
{
    /// <summary>
    /// 掌を上向き（仰向け）にすると窓を手の位置に登場させる。
    /// 登場後 PinchOpenDoors を有効化する。
    /// </summary>
    public class PalmSummonWindow : MonoBehaviour
    {
        [Header("登場させる窓オブジェクト")]
        [SerializeField] GameObject m_WindowRoot;

        [Header("掌検知")]
        [Tooltip("掌の法線と WorldUp の内積がこれ以上で「上向き」と判定")]
        [SerializeField] float m_PalmUpDot = 0.7f;
        [Tooltip("この秒数以上キープしたら登場")]
        [SerializeField] float m_HoldSeconds = 0.8f;

        [Header("登場位置オフセット (カメラ前方に出す)")]
        [Tooltip("カメラからの前方距離 (m)")]
        [SerializeField] float m_CameraDistance = 0.6f;
        [Tooltip("カメラ位置からの上下オフセット (m)。負で下げる")]
        [SerializeField] float m_HeightOffset = -0.1f;

        [Header("次のステップ")]
        [SerializeField] PinchOpenDoors m_PinchOpenDoors;

        [Header("デバッグ")]
        [SerializeField] bool m_DebugLog = true;

        XRHandSubsystem m_Subsystem;
        float m_HoldTimer;
        bool m_Summoned;

        void Start()
        {
            if (m_WindowRoot != null) m_WindowRoot.SetActive(false); // 初期非表示はDissolveなし
            if (m_PinchOpenDoors != null) m_PinchOpenDoors.enabled = false;
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
#if UNITY_EDITOR
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) { EditorReset(); return; }
#endif

            if (m_Summoned) return;

#if UNITY_EDITOR
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Summon();
                return;
            }
#endif

            var subsystem = GetSubsystem();
            if (subsystem == null)
            {
                if (m_DebugLog && Time.frameCount % 120 == 0)
                    Debug.Log("[PalmSummon] XRHandSubsystem not available yet...");
                return;
            }

            if (IsPalmUpDetected(subsystem))
            {
                m_HoldTimer += Time.deltaTime;
                if (m_DebugLog && Time.frameCount % 30 == 0)
                    Debug.Log($"[PalmSummon] Palm-up! hold={m_HoldTimer:F2}/{m_HoldSeconds}s");
                if (m_HoldTimer >= m_HoldSeconds)
                    Summon();
            }
            else
            {
                if (m_HoldTimer > 0f && m_DebugLog)
                    Debug.Log("[PalmSummon] Palm-up lost, resetting timer.");
                m_HoldTimer = 0f;
            }
        }

        bool IsPalmUpDetected(XRHandSubsystem subsystem)
        {
            return IsPalmUp(subsystem.leftHand) || IsPalmUp(subsystem.rightHand);
        }

        bool IsPalmUp(XRHand hand)
        {
            if (!hand.isTracked) return false;
            var joint = hand.GetJoint(XRHandJointID.Palm);
            if (!joint.TryGetPose(out var p)) return false;

            // -p.up = 掌の法線（手のひらが向いている方向）。p.up は手の甲方向のため反転する
            float dotUp = Vector3.Dot(-p.up, Vector3.up);

            if (m_DebugLog && Time.frameCount % 60 == 0)
                Debug.Log($"[PalmSummon] {hand.handedness} palmNormal.up={dotUp:F2} (threshold={m_PalmUpDot})");

            return dotUp >= m_PalmUpDot;
        }

        public bool  Summoned    => m_Summoned;
        public float HoldTimer   => m_HoldTimer;
        public float HoldSeconds => m_HoldSeconds;
        public float PalmUpDot   => m_PalmUpDot;

        public void Summon()
        {
            m_Summoned = true;
            if (m_WindowRoot == null) return;

            // ARトラッカー親の影響を受けないようシーンルートへ
            m_WindowRoot.transform.SetParent(null, true);

            var cam = Camera.main;
            if (cam == null) return;

            var pos = cam.transform.position
                + cam.transform.forward * m_CameraDistance
                + Vector3.up * m_HeightOffset;
            m_WindowRoot.transform.position = pos;
            m_WindowRoot.transform.rotation = Quaternion.LookRotation(cam.transform.forward, Vector3.up);

            var dissolve = m_WindowRoot.GetComponent<DissolveEffect>();
            if (dissolve != null) dissolve.Show();
            else m_WindowRoot.SetActive(true);

            if (m_PinchOpenDoors != null) m_PinchOpenDoors.enabled = true;
            if (m_DebugLog) Debug.Log($"[PalmSummon] Window summoned at {pos}. PinchOpenDoors enabled.");
        }

        public void EditorReset()
        {
            m_Summoned = false;
            m_HoldTimer = 0f;
            if (m_WindowRoot != null)
            {
                var dissolve = m_WindowRoot.GetComponent<DissolveEffect>();
                if (dissolve != null) dissolve.Hide();
                else m_WindowRoot.SetActive(false);
            }
            if (m_PinchOpenDoors != null) m_PinchOpenDoors.enabled = false;
            if (m_DebugLog) Debug.Log("[PalmSummon] Editor reset.");
        }
    }
}
