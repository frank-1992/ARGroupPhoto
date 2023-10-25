using UnityEngine;
using UnityEditor;

namespace MVXUnity
{
    [CustomEditor(typeof(MvxDataStreamDeterminer), editorForChildClasses: true), CanEditMultipleObjects]
    public class MvxDataStreamDeterminerEditor : MvxDataStreamEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawPlaybackModeProperty();
            DrawFollowFPSProperty();
            DrawMinimalBufferedAudioDurationProperty();
            DrawAudioStreamEnabledProperty();
        }

        private void DrawPlaybackModeProperty()
        {
            Mvx2API.RunnerPlaybackMode playbackMode = ((MvxDataStreamDeterminer)target).playbackMode;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxDataStreamDeterminer)targetObject).playbackMode != playbackMode;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            Mvx2API.RunnerPlaybackMode newPlaybackMode = (Mvx2API.RunnerPlaybackMode) EditorGUILayout.EnumPopup(new GUIContent("Playback mode"), playbackMode);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (playbackMode != newPlaybackMode)
            {
                foreach (object targetObject in targets)
                    ((MvxDataStreamDeterminer)targetObject).playbackMode = newPlaybackMode;
            }
        }

        private void DrawFollowFPSProperty()
        {
            bool followStreamFPS = ((MvxDataStreamDeterminer)target).followStreamFPS;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxDataStreamDeterminer)targetObject).followStreamFPS != followStreamFPS;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            bool newFollowStreamFPS = EditorGUILayout.Toggle(new GUIContent("Follow stream FPS"), followStreamFPS);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (followStreamFPS != newFollowStreamFPS)
            {
                foreach (object targetObject in targets)
                    ((MvxDataStreamDeterminer)targetObject).followStreamFPS = newFollowStreamFPS;
            }
        }

        private void DrawMinimalBufferedAudioDurationProperty()
        {
            float minimalBufferedAudioDuration = ((MvxDataStreamDeterminer)target).minimalBufferedAudioDuration;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxDataStreamDeterminer)targetObject).minimalBufferedAudioDuration != minimalBufferedAudioDuration;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            float newMinimalBufferedAudioDuration = EditorGUILayout.FloatField(new GUIContent("Minimal buffered audio duration"), minimalBufferedAudioDuration);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (minimalBufferedAudioDuration != newMinimalBufferedAudioDuration)
            {
                foreach (object targetObject in targets)
                    ((MvxDataStreamDeterminer)targetObject).minimalBufferedAudioDuration = newMinimalBufferedAudioDuration;
            }
        }

        private void DrawAudioStreamEnabledProperty()
        {
            bool audioStreamEnabled = ((MvxDataStreamDeterminer)target).audioStreamEnabled;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxDataStreamDeterminer)targetObject).audioStreamEnabled != audioStreamEnabled;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            bool newAudioStreamEnabled = EditorGUILayout.Toggle(new GUIContent("Audio stream enabled"), audioStreamEnabled);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (audioStreamEnabled != newAudioStreamEnabled)
            {
                foreach (object targetObject in targets)
                    ((MvxDataStreamDeterminer)targetObject).audioStreamEnabled = newAudioStreamEnabled;
            }
        }
    }
}