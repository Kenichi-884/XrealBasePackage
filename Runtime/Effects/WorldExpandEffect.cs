using System.Collections;
using UnityEngine;

namespace XrealBase.Effects
{
    public class WorldExpandEffect : MonoBehaviour
    {
        [SerializeField]
        float m_Duration = 2f;

        [SerializeField]
        AnimationCurve m_EaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        Vector3 m_TargetScale;

        void Awake()
        {
            m_TargetScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }

        void Start()
        {
            StartCoroutine(ExpandRoutine());
        }

        IEnumerator ExpandRoutine()
        {
            var elapsed = 0f;

            while (elapsed < m_Duration)
            {
                elapsed += Time.deltaTime;
                var t = m_EaseCurve.Evaluate(Mathf.Clamp01(elapsed / m_Duration));
                transform.localScale = m_TargetScale * t;
                yield return null;
            }

            transform.localScale = m_TargetScale;
        }
    }
}
