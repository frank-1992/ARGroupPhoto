using UnityEngine;
using UnityEditor;

namespace MVXUnity
{
    [CustomEditor(typeof(MvxDataRandomAccessStream), editorForChildClasses: true), CanEditMultipleObjects]
    public class MvxDataRandomAccessStreamEditor : MvxDataStreamEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawFrameIDProperty();
        }

        private void DrawFrameIDProperty()
        {
            uint frameId = ((MvxDataRandomAccessStream)target).frameId;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxDataRandomAccessStream)targetObject).frameId != frameId;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            uint newFrameId = (uint)EditorGUILayout.IntField(new GUIContent("Frame ID"), (int)frameId);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (frameId != newFrameId)
            {
                foreach (object targetObject in targets)
                    ((MvxDataRandomAccessStream)targetObject).frameId = newFrameId;
            }
        }
    }
}