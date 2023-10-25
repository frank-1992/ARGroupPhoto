using UnityEngine;
using UnityEditor;

namespace MVXUnity
{
    [CustomEditor(typeof(MvxAsyncDataProcessor), editorForChildClasses:true), CanEditMultipleObjects]
    public class MvxAsyncDataProcessorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "MvxAsyncDataProcessor properties changed");

            DrawDefaultInspector();
            DrawMvxStreamProperty();
        }

        private void DrawMvxStreamProperty()
        {
            MvxDataStream mvxStream = ((MvxAsyncDataProcessor)target).mvxStream;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxAsyncDataProcessor)targetObject).mvxStream != mvxStream;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            MvxDataStream newMvxStream = (MvxDataStream) EditorGUILayout.ObjectField(new GUIContent("Mvx stream"), mvxStream, typeof(MvxDataStream), true);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (mvxStream != newMvxStream)
            {
                foreach (object targetObject in targets)
                    ((MvxAsyncDataProcessor)targetObject).mvxStream = newMvxStream;
            }
        }
    }
}
