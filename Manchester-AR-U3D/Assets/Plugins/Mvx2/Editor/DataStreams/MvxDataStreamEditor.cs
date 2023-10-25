using UnityEngine;
using UnityEditor;

namespace MVXUnity
{
    [CustomEditor(typeof(MvxDataStream), editorForChildClasses: true), CanEditMultipleObjects]
    public class MvxDataStreamEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "MvxDataStream properties changed");

            DrawDefaultInspector();
            DrawDataStreamDefinitionProperty();
        }

        private void DrawDataStreamDefinitionProperty()
        {
            MvxDataStreamDefinition dataStreamDefinition = ((MvxDataStream)target).dataStreamDefinition;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxDataStream)targetObject).dataStreamDefinition != dataStreamDefinition;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            MvxDataStreamDefinition newDataStreamDefinition = (MvxDataStreamDefinition)EditorGUILayout.ObjectField(new GUIContent("Data stream definition"), dataStreamDefinition, typeof(MvxDataStreamDefinition), true);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (dataStreamDefinition != newDataStreamDefinition)
            {
                foreach (object targetObject in targets)
                    ((MvxDataStream)targetObject).dataStreamDefinition = newDataStreamDefinition;
            }
        }
    }
}