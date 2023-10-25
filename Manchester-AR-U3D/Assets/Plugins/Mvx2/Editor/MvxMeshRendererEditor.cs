using UnityEngine;
using UnityEditor;

namespace MVXUnity
{
    [CustomEditor(typeof(MvxMeshRenderer), editorForChildClasses:true), CanEditMultipleObjects]
    public class MvxMeshRendererEditor : MvxAsyncDataProcessorEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawMaterialTemplatesProperty();
        }

        private void DrawMaterialTemplatesProperty()
        {
            if (targets.Length > 1)
            {
                EditorGUILayout.LabelField(new GUIContent("Material templates"));
                using (new EditorGUI.IndentLevelScope())
                    EditorGUILayout.HelpBox("Multi-editing not supported for the material templates collection", MessageType.Warning);
                return;
            }

            Material[] materialTemplates = ((MvxMeshRenderer)target).materialTemplates;
            Material[] newMaterialTemplates = DrawMaterialTemplatesInspector(materialTemplates);
            
            if (!MaterialTemplatesCollectionsAreEqual(newMaterialTemplates, materialTemplates))
                ((MvxMeshRenderer)target).materialTemplates = newMaterialTemplates;
        }

        private bool MaterialTemplatesCollectionsAreEqual(Material[] materialTemplates1, Material[] materialTemplates2)
        {
            if (materialTemplates1 == null && materialTemplates2 == null)
                return true;

            if (materialTemplates1 == null || materialTemplates2 == null)
                return false;

            if (materialTemplates1.Length != materialTemplates2.Length)
                return false;

            for (int i = 0; i < materialTemplates1.Length; i++)
            {
                if (materialTemplates1[i] != materialTemplates2[i])
                    return false;
            }

            return true;
        }

        private string m_materialTemplatesCountStr = "0";

        private Material[] DrawMaterialTemplatesInspector(Material[] materialTemplates)
        {
            m_materialTemplatesCountStr = materialTemplates == null ? "0" : materialTemplates.Length.ToString();
            Material[] newMaterialTemplates = null;

            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("Material templates"));

                    m_materialTemplatesCountStr = EditorGUILayout.TextField(m_materialTemplatesCountStr, GUILayout.ExpandWidth(false));

                    int materialTemplatesCount;
                    if (System.Int32.TryParse(m_materialTemplatesCountStr, out materialTemplatesCount))
                        newMaterialTemplates = new Material[materialTemplatesCount];
                    else
                        newMaterialTemplates = new Material[materialTemplates == null ? 0 : materialTemplates.Length];
                }

                using (new EditorGUI.IndentLevelScope())
                {
                    for (int i = 0; i < newMaterialTemplates.Length; i++)
                    {
                        Material originalMaterialTemplate = materialTemplates == null || i >= materialTemplates.Length ? null : materialTemplates[i];
                        newMaterialTemplates[i] = (Material)EditorGUILayout.ObjectField(originalMaterialTemplate, typeof(Material), false);
                    }
                }
            }

            return newMaterialTemplates;
        }
    }
}
