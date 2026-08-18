using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace XrealBase.Interaction
{
    /// <summary>
    /// XRGrabInteractable のつかむ/離すイベントを UnityEvent として公開する薄いラッパー。
    /// 他コンポーネントを Inspector で配線するだけで使える。
    /// </summary>
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
    public class GrabbableEventSender : MonoBehaviour
    {
        [System.Serializable] public class Vector3Event : UnityEvent<Vector3> { }

        [Header("Events")]
        [Tooltip("つかんだとき。引数: インタラクターのワールド座標")]
        public Vector3Event onGrabbed  = new();
        [Tooltip("離したとき")]
        public UnityEvent   onReleased = new();

        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable m_Interactable;

        void Awake() => m_Interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        void OnEnable()
        {
            m_Interactable.selectEntered.AddListener(OnSelectEntered);
            m_Interactable.selectExited.AddListener(OnSelectExited);
        }

        void OnDisable()
        {
            m_Interactable.selectEntered.RemoveListener(OnSelectEntered);
            m_Interactable.selectExited.RemoveListener(OnSelectExited);
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            var pos = args.interactorObject is Component c ? c.transform.position : transform.position;
            onGrabbed.Invoke(pos);
        }

        void OnSelectExited(SelectExitEventArgs _) => onReleased.Invoke();
    }
}
