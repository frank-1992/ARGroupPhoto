using UnityEngine;
using UnityEditor;

namespace MVXUnity
{
    [CustomEditor(typeof(MvxDataStreamThumbnail), editorForChildClasses: true)]
    public class MvxDataStreamThumbnailEditor : MvxDataStreamEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawFrameIDProperty();
        }

        private void DrawFrameIDProperty()
        {
            MvxDataStreamThumbnail stream = (MvxDataStreamThumbnail)target;

            if (!stream.isOpen || stream.mvxSourceInfo == null)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                    EditorGUILayout.IntSlider(new GUIContent("Frame ID"), 0, 0, 0);
                return;
            }

            int framesCount = (int)stream.mvxSourceInfo.GetNumFrames();
            uint frameId = stream.frameId;
            uint newFrameId = (uint)EditorGUILayout.IntSlider(new GUIContent("Frame ID"), (int)frameId, 0, framesCount - 1);

            if (frameId != newFrameId)
            {
                stream.frameId = newFrameId;
                EditorUtility.SetDirty(target);
            }
        }
    }
}