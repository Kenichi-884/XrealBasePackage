using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XrealBase.Effects
{
    /// <summary>
    /// Dissolve Burn シェーダーの Dissolve Amount を制御して
    /// GameObject をフェードイン / フェードアウトさせる。
    /// Show() : SetActive(true) → Dissolve Amount 1→0
    /// Hide() : Dissolve Amount 0→1 → SetActive(false)
    ///
    /// Dissolve Burn シェーダーが当たっていない Renderer は
    /// Show/Hide のタイミングで enabled を切り替えるフォールバックを使用。
    /// </summary>
    public class DissolveEffect : MonoBehaviour
    {
        [Tooltip("対象 Renderer。空のとき子を含む全 Renderer を自動取得")]
        [SerializeField] Renderer[] m_Renderers;

        [Tooltip("シェーダーの Dissolve Amount プロパティ名")]
        [SerializeField] string m_DissolveProperty = "_DissolveAmount";

        [Tooltip("フェードイン / フェードアウト時間（秒）")]
        [SerializeField] float m_Duration = 1f;

        readonly List<Renderer> m_DissolveRenderers = new();
        readonly List<Renderer> m_FallbackRenderers  = new();

        Coroutine m_Coroutine;

        void Awake()
        {
            if (m_Renderers == null || m_Renderers.Length == 0)
                m_Renderers = GetComponentsInChildren<Renderer>(true);

            foreach (var r in m_Renderers)
            {
                if (r == null) continue;
                bool hasDissolve = false;
                foreach (var mat in r.sharedMaterials)
                    if (mat != null && mat.HasProperty(m_DissolveProperty)) { hasDissolve = true; break; }

                if (hasDissolve) m_DissolveRenderers.Add(r);
                else             m_FallbackRenderers.Add(r);
            }
        }

        /// <summary>Dissolve しながら表示する。</summary>
        public void Show()
        {
            if (m_Coroutine != null) StopCoroutine(m_Coroutine);
            // 非アクティブ中は StartCoroutine が呼べないため、先に SetActive(true)
            gameObject.SetActive(true);
            m_Coroutine = StartCoroutine(ShowRoutine());
        }

        /// <summary>Dissolve しながら非表示にする。</summary>
        public void Hide()
        {
            if (m_Coroutine != null) StopCoroutine(m_Coroutine);
            m_Coroutine = StartCoroutine(HideRoutine());
        }

        IEnumerator ShowRoutine()
        {
            foreach (var r in m_FallbackRenderers) if (r != null) r.enabled = true;
            SetDissolve(1f);

            var elapsed = 0f;
            while (elapsed < m_Duration)
            {
                elapsed += Time.deltaTime;
                SetDissolve(Mathf.Lerp(1f, 0f, elapsed / m_Duration));
                yield return null;
            }

            SetDissolve(0f);
            m_Coroutine = null;
        }

        IEnumerator HideRoutine()
        {
            foreach (var r in m_FallbackRenderers) if (r != null) r.enabled = false;
            SetDissolve(0f);

            var elapsed = 0f;
            while (elapsed < m_Duration)
            {
                elapsed += Time.deltaTime;
                SetDissolve(Mathf.Lerp(0f, 1f, elapsed / m_Duration));
                yield return null;
            }

            SetDissolve(1f);
            gameObject.SetActive(false);
            m_Coroutine = null;
        }

        void SetDissolve(float value)
        {
            foreach (var r in m_DissolveRenderers)
            {
                if (r == null) continue;
                foreach (var mat in r.materials)
                    mat.SetFloat(m_DissolveProperty, value);
            }
        }
    }
}
