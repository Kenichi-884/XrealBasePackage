using UnityEngine;
using XrealBase.Effects;

namespace XrealBase.Scene
{
    /// <summary>
    /// EventSceneDirector.onSceneEntered に反応して
    /// 指定シーンID に入ったとき / 離れたときに自身の GameObject を表示・非表示にする。
    /// Window など「特定シーンでだけ見せたい」オブジェクトにアタッチして使う。
    /// </summary>
    public class SceneActivation : MonoBehaviour
    {
        [SerializeField] EventSceneDirector m_Director;

        [Tooltip("これらのシーンIDに入ったとき非表示にする")]
        [SerializeField] string[] m_HideOnSceneIds = new string[0];

        [Tooltip("これらのシーンIDに入ったとき表示する")]
        [SerializeField] string[] m_ShowOnSceneIds = new string[0];

        void Start()
        {
            if (m_Director == null)
                m_Director = FindObjectOfType<EventSceneDirector>();

            if (m_Director != null)
                m_Director.onSceneEntered.AddListener(OnSceneEntered);
        }

        void OnDestroy()
        {
            if (m_Director != null)
                m_Director.onSceneEntered.RemoveListener(OnSceneEntered);
        }

        void OnSceneEntered(string sceneId)
        {
            var dissolve = GetComponent<DissolveEffect>();

            if (System.Array.IndexOf(m_HideOnSceneIds, sceneId) >= 0)
            {
                if (dissolve != null) dissolve.Hide();
                else gameObject.SetActive(false);
            }
            else if (System.Array.IndexOf(m_ShowOnSceneIds, sceneId) >= 0)
            {
                if (dissolve != null) dissolve.Show();
                else gameObject.SetActive(true);
            }
        }
    }
}
