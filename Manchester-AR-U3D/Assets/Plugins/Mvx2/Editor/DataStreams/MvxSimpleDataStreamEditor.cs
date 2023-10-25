using UnityEngine;
using UnityEditor;

namespace MVXUnity
{
    [CustomEditor(typeof(MvxSimpleDataStream), editorForChildClasses: true), CanEditMultipleObjects]
    public class MvxSimpleDataStreamEditor : MvxDataStreamEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawPlaybackModeProperty();
            DrawFollowFPSProperty();
        }

        private void DrawPlaybackModeProperty()
        {
            Mvx2API.RunnerPlaybackMode playbackMode = ((MvxSimpleDataStream)target).playbackMode;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxSimpleDataStream)targetObject).playbackMode != playbackMode;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            Mvx2API.RunnerPlaybackMode newPlaybackMode = (Mvx2API.RunnerPlaybackMode) EditorGUILayout.EnumPopup(new GUIContent("Playback mode"), playbackMode);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (playbackMode != newPlaybackMode)
            {
                foreach (object targetObject in targets)
                    ((MvxSimpleDataStream)targetObject).playbackMode = newPlaybackMode;
            }
        }

        private void DrawFollowFPSProperty()
        {
            bool followStreamFPS = ((MvxSimpleDataStream)target).followStreamFPS;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxSimpleDataStream)targetObject).followStreamFPS != followStreamFPS;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            bool newFollowStreamFPS = EditorGUILayout.Toggle(new GUIContent("Follow stream FPS"), followStreamFPS);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (followStreamFPS != newFollowStreamFPS)
            {
                foreach (object targetObject in targets)
                    ((MvxSimpleDataStream)targetObject).followStreamFPS = newFollowStreamFPS;
            }
        }
    }
}