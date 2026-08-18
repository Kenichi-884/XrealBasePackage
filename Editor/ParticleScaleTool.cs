using UnityEditor;
using UnityEngine;

namespace XrealBase.Editor
{
    /// <summary>
    /// 選択オブジェクトの Transform.localScale を変えることでパーティクルを非破壊スケール。
    /// 各 ParticleSystem の scalingMode を Hierarchy に設定することで親スケールを反映させる。
    /// 元のパーティクルパラメータは一切変更しない。
    /// </summary>
    public class ParticleScaleTool : EditorWindow
    {
        float m_Scale = 0.5f;

        [MenuItem("Tools/XrealBase/Particle Scale Tool")]
        static void Open() => GetWindow<ParticleScaleTool>("Particle Scale");

        void OnGUI()
        {
            EditorGUILayout.LabelField("選択オブジェクトの Transform スケールを変更（非破壊）", EditorStyles.wordWrappedLabel);
            EditorGUILayout.HelpBox("ParticleSystem パラメータは変更しません。\nscalingMode を Hierarchy にして親スケールで制御します。", MessageType.Info);
            EditorGUILayout.Space(8);

            m_Scale = EditorGUILayout.FloatField("Scale 倍率", m_Scale);

            EditorGUILayout.Space(8);

            GUI.enabled = Selection.gameObjects.Length > 0 && m_Scale > 0f;
            if (GUILayout.Button("Apply"))
                Apply();
            GUI.enabled = true;

            if (Selection.gameObjects.Length == 0)
                EditorGUILayout.HelpBox("Hierarchy でオブジェクトを選択してください。", MessageType.Info);
        }

        void Apply()
        {
            int psCount = 0;
            foreach (var go in Selection.gameObjects)
            {
                Undo.RecordObject(go.transform, "Particle Scale (Transform)");
                go.transform.localScale *= m_Scale;

                foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
                {
                    Undo.RecordObject(ps, "Particle Scale (ScalingMode)");
                    var main = ps.main;
                    main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                    EditorUtility.SetDirty(ps);
                    psCount++;
                }

                EditorUtility.SetDirty(go);
            }
            Debug.Log($"[ParticleScaleTool] {Selection.gameObjects.Length} オブジェクトに ×{m_Scale} を適用（ParticleSystem {psCount} 個の scalingMode → Hierarchy）");
        }
    }
}
