using UnityEngine;

namespace XrealBase.Interaction
{
    /// <summary>
    /// 起動時（または OnEnable 時）にこの GameObject をカメラ正面に配置する。
    /// イントロシーンの Window にアタッチして使う。
    /// </summary>
    public class IntroWindowPlacer : MonoBehaviour
    {
        [SerializeField] float  m_Distance     = 1.5f;
        [SerializeField] float  m_HeightOffset = 0f;
        [Tooltip("空欄のとき Camera.main を自動取得")]
        [SerializeField] Camera m_Camera;

        void Start()
        {
            PlaceInFrontOfCamera();
        }

        public void PlaceInFrontOfCamera()
        {
            if (m_Camera == null) m_Camera = Camera.main;
            if (m_Camera == null) return;

            var camTransform = m_Camera.transform;
            var forward      = camTransform.forward;
            forward.y        = 0f;

            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            else
                forward.Normalize();

            transform.position = camTransform.position
                               + forward * m_Distance
                               + Vector3.up * m_HeightOffset;

            // 窓の正面をカメラに向ける
            transform.rotation = Quaternion.LookRotation(-forward);
        }
    }
}
