using UnityEngine;
using UnityEditor;

namespace MVXUnity
{
    [CustomEditor(typeof(MvxCustomDataStreamDefinition), editorForChildClasses: true), CanEditMultipleObjects]
    public class MvxCustomDataStreamDefinitionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawApplyButton();
        }

        private void DrawApplyButton()
        {
            if (GUILayout.Button("Apply"))
            {
                foreach (object targetObject in targets)
                {
                    ((MvxCustomDataStreamDefinition)targetObject).Apply();
                    EditorUtility.SetDirty(((MvxCustomDataStreamDefinition)targetObject));
                }
            }
        }
    }
}