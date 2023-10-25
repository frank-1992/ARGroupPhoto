using UnityEngine;
using UnityEditor;

namespace MVXUnity
{
    [CustomEditor(typeof(MvxFileDataStreamDefinition), editorForChildClasses: true), CanEditMultipleObjects]
    public class MvxFileDataStreamDefinitionEditor : Editor
    {
        private readonly GUIContent m_filePathGuiContent = new GUIContent("File path",
            "Path to MVX2 file. Can be full path or relative to StreamingAssets folder.");

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "MvxFileDataStreamDefinition properties changed");

            DrawDefaultInspector();
            DrawFilePathProperty();
        }

        private bool m_droppingFilePath = false;

        private void DrawFilePathProperty()
        {
            string filePath = ((MvxFileDataStreamDefinition)target).filePath;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxFileDataStreamDefinition)targetObject).filePath != filePath;

            string newFilePath;
            Rect filePathFieldRect;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            {
                filePathFieldRect = EditorGUILayout.BeginVertical();
                {
                    Color originalBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = m_droppingFilePath ? Color.green : originalBackgroundColor;
                    {
                        newFilePath = EditorGUILayout.TextField(m_filePathGuiContent, filePath);
                    }
                    GUI.backgroundColor = originalBackgroundColor;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.showMixedValue = originalShowMixedValue;

            CheckFilePathDropEvent(filePathFieldRect, ref newFilePath);

            if (filePath != newFilePath)
            {
                foreach (object targetObject in targets)
                {
                    ((MvxFileDataStreamDefinition)targetObject).filePath = newFilePath;
                    EditorUtility.SetDirty(((MvxFileDataStreamDefinition)targetObject));
                }
            }
        }

        private static bool IsValidFilePath(string path)
        {
            return System.IO.Path.IsPathRooted(path) || path.StartsWith("Assets/StreamingAssets/");
        }

        private static string GetValidFilePath(string path)
        {
            if (System.IO.Path.IsPathRooted(path))
                return path;
            else if (path.StartsWith("Assets/StreamingAssets/"))
                return path.Replace("Assets/StreamingAssets/", "");
            else
                return "";
        }

        private void CheckFilePathDropEvent(Rect dropRect, ref string filePath)
        {
            Event evt = Event.current;

            if (evt.type == EventType.DragUpdated)
            {
                m_droppingFilePath = false;

                if (dropRect.Contains(evt.mousePosition))
                {
                    if (DragAndDrop.paths.Length == 1 && IsValidFilePath(DragAndDrop.paths[0]))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        m_droppingFilePath = true;
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                }
            }
            else if (evt.type == EventType.DragUpdated)
            {
                m_droppingFilePath = false;

                if (dropRect.Contains(evt.mousePosition))
                {
                    if (DragAndDrop.paths.Length == 1 && IsValidFilePath(DragAndDrop.paths[0]))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        m_droppingFilePath = true;
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                }
            }
            else if (evt.type == EventType.DragPerform)
            {
                m_droppingFilePath = false;

                if (dropRect.Contains(evt.mousePosition))
                {
                    if (DragAndDrop.paths.Length == 1 && IsValidFilePath(DragAndDrop.paths[0]))
                        filePath = GetValidFilePath(DragAndDrop.paths[0]);
                }
            }
            else if (evt.type == EventType.DragExited)
            {
                m_droppingFilePath = false;
            }
        }
    }
}