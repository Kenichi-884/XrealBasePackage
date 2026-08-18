using UnityEngine;
using UnityEngine.Events;

namespace XrealBase.Interaction
{
    /// <summary>
    /// 2枚のドアパネルがローカルY回転で ±m_OpenThreshold 以上開いたら
    /// onWindowOpened を発火して次のシーンへ進む。
    /// ドア自体の回転は XRGrabInteractable 等に任せ、ここでは監視のみ行う。
    /// </summary>
    public class WindowGimmick : MonoBehaviour
    {
        [SerializeField] Transform m_DoorL;
        [SerializeField] Transform m_DoorR;

        [Tooltip("この角度(絶対値)以上でドアが「開いた」と判定 (°)")]
        [SerializeField] float m_OpenThreshold = 100f;

        public UnityEvent onWindowOpened;

        bool m_Opened;

        // GrabbableEventSender からの旧 API（後方互換）
        public void OnGrabStarted(Vector3 _) { }
        public void OnReleased() { }

        void Update()
        {
            if (m_Opened) return;
            if (IsOpen(m_DoorL) && IsOpen(m_DoorR))
            {
                m_Opened = true;
                onWindowOpened.Invoke();
            }
        }

        bool IsOpen(Transform door)
        {
            if (door == null) return false;
            float y = door.localEulerAngles.y;
            if (y > 180f) y -= 360f;   // 0..360 → -180..180 に正規化
            return Mathf.Abs(y) >= m_OpenThreshold;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            DrawStatus(m_DoorL);
            DrawStatus(m_DoorR);
        }

        void DrawStatus(Transform door)
        {
            if (door == null) return;
            Gizmos.color = IsOpen(door) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(door.position, 0.05f);
        }
#endif
    }
}
