using UnityEngine;
using UnityEditor;

namespace MVXUnity
{
    [CustomEditor(typeof(MvxAudioPlayerStream), editorForChildClasses: true), CanEditMultipleObjects]
    public class MvxAudioPlayerStreamEditor : MvxDataStreamEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawPlaybackModeProperty();
        }

        private void DrawPlaybackModeProperty()
        {
            MvxAudioPlayerStream.AudioPlaybackMode playbackMode = ((MvxAudioPlayerStream)target).playbackMode;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxAudioPlayerStream)targetObject).playbackMode != playbackMode;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            MvxAudioPlayerStream.AudioPlaybackMode newPlaybackMode = (MvxAudioPlayerStream.AudioPlaybackMode) EditorGUILayout.EnumPopup(new GUIContent("Playback mode"), playbackMode);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (playbackMode != newPlaybackMode)
            {
                foreach (object targetObject in targets)
                    ((MvxAudioPlayerStream)targetObject).playbackMode = newPlaybackMode;
            }
        }
    }
}