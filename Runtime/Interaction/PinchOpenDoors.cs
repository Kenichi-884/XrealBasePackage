using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands;
using XrealBase.Effects;

namespace XrealBase.Interaction
{
    /// <summary>
    /// ピンチ動作を検知して左右ドアを自動アニメで開く。
    /// 両方が目標角度に達したら onDoorsOpened を発火する。
    /// PalmSummonWindow から enabled = true にして起動する。
    /// </summary>
    public class PinchOpenDoors : MonoBehaviour
    {
        [Header("ドア Transform")]
        [SerializeField] Transform m_DoorL;
        [SerializeField] Transform m_DoorR;

        [Header("ドアを開いたときに表示するオブジェクト")]
        [SerializeField] GameObject m_RevealObject;

        [Header("目標角度 (ローカルY°)")]
        [SerializeField] float m_TargetAngleL =  120f;
        [SerializeField] float m_TargetAngleR = -120f;

        [Header("アニメ速度")]
        [SerializeField] float m_DegreesPerSec = 80f;

        [Header("ピンチ判定")]
        [Tooltip("親指先端と人差し指先端の距離がこれ未満でピンチと判定 (m)")]
        [SerializeField] float m_PinchThreshold = 0.025f;
        [Tooltip("有効化直後のこの秒数は検知しない（誤検知防止）")]
        [SerializeField] float m_StartupDelay = 0.5f;

        [Header("SFX")]
        [SerializeField] AudioClip m_OpenSFX;
        [SerializeField] AudioSource m_AudioSource;

        [Header("デバッグ")]
        [SerializeField] bool m_DebugLog = true;

        public UnityEvent onDoorsOpened;

        XRHandSubsystem m_Subsystem;
        bool m_Opening;
        bool m_Opened;
        bool m_WasPinching;
        float m_AngleL;
        float m_AngleR;
        float m_EnabledTimer;

        public bool  Opening        => m_Opening;
        public bool  Opened         => m_Opened;
        public float EnabledTimer   => m_EnabledTimer;
        public float StartupDelay   => m_StartupDelay;
        public float PinchThreshold => m_PinchThreshold;

        XRHandSubsystem GetSubsystem()
        {
            if (m_Subsystem != null && m_Subsystem.running) return m_Subsystem;
            var list = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(list);
            m_Subsystem = list.Count > 0 ? list[0] : null;
            return m_Subsystem;
        }

        void OnEnable()
        {
            m_AngleL = 0f;
            m_AngleR = 0f;
            m_Opening = false;
            m_Opened = false;
            m_WasPinching = false;
            m_EnabledTimer = 0f;

            // ドアを閉じた状態にリセット
            if (m_DoorL != null) m_DoorL.localEulerAngles = Vector3.zero;
            if (m_DoorR != null) m_DoorR.localEulerAngles = Vector3.zero;

            if (m_DebugLog) Debug.Log("[PinchDoors] Enabled — doors reset. Waiting for pinch.");
        }

        void Update()
        {
            if (m_Opened) return;

            m_EnabledTimer += Time.deltaTime;

#if UNITY_EDITOR
            if (!m_Opening && Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            {
                m_Opening = true;
                PlayOpenSFX();
                if (m_DebugLog) Debug.Log("[PinchDoors] Editor P key: starting door open.");
            }
#endif

            if (!m_Opening && m_EnabledTimer >= m_StartupDelay)
            {
                bool pinching = IsPinching();
                if (pinching && !m_WasPinching)  // 立ち上がりエッジのみ（ピンチし始めた瞬間）
                {
                    m_Opening = true;
                    PlayOpenSFX();
                    if (m_DebugLog) Debug.Log("[PinchDoors] Pinch onset detected! Starting door animation.");
                }
                m_WasPinching = pinching;
            }

            if (!m_Opening) return;

            m_AngleL = Mathf.MoveTowards(m_AngleL, m_TargetAngleL, m_DegreesPerSec * Time.deltaTime);
            m_AngleR = Mathf.MoveTowards(m_AngleR, m_TargetAngleR, m_DegreesPerSec * Time.deltaTime);

            if (m_DoorL != null) m_DoorL.localEulerAngles = new Vector3(0f, m_AngleL, 0f);
            if (m_DoorR != null) m_DoorR.localEulerAngles = new Vector3(0f, m_AngleR, 0f);

            if (Mathf.Abs(m_AngleL - m_TargetAngleL) < 0.1f &&
                Mathf.Abs(m_AngleR - m_TargetAngleR) < 0.1f)
            {
                m_Opened = true;
                if (m_RevealObject != null)
                {
                    var dissolve = m_RevealObject.GetComponent<DissolveEffect>();
                    if (dissolve != null) dissolve.Show();
                    else m_RevealObject.SetActive(true);
                }
                if (m_DebugLog) Debug.Log("[PinchDoors] Doors fully opened! Firing onDoorsOpened.");
                onDoorsOpened.Invoke();
            }
        }

        bool IsPinching()
        {
            var subsystem = GetSubsystem();
            if (subsystem == null) return false;

            foreach (var hand in new[] { subsystem.leftHand, subsystem.rightHand })
            {
                if (!hand.isTracked) continue;

                var thumbJoint = hand.GetJoint(XRHandJointID.ThumbTip);
                var indexJoint = hand.GetJoint(XRHandJointID.IndexTip);

                if (thumbJoint.TryGetPose(out var thumb) && indexJoint.TryGetPose(out var index))
                {
                    float dist = Vector3.Distance(thumb.position, index.position);
                    if (m_DebugLog && Time.frameCount % 30 == 0)
                        Debug.Log($"[PinchDoors] {hand.handedness} dist={dist * 100f:F1}cm (threshold={m_PinchThreshold * 100f:F1}cm)");
                    if (dist < m_PinchThreshold)
                        return true;
                }
            }
            return false;
        }

        public void EditorTriggerOpen()
        {
            if (m_Opening || m_Opened) return;
            m_Opening = true;
            PlayOpenSFX();
            if (m_DebugLog) Debug.Log("[PinchDoors] Editor triggered: starting door open.");
        }

        void PlayOpenSFX()
        {
            if (m_OpenSFX == null) return;
            if (m_AudioSource == null)
                m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.PlayOneShot(m_OpenSFX);
        }
    }
}
