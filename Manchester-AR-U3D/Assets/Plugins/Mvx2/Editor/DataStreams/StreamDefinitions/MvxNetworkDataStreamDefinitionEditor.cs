using UnityEngine;
using UnityEditor;

namespace MVXUnity
{
    [CustomEditor(typeof(MvxNetworkDataStreamDefinition), editorForChildClasses: true), CanEditMultipleObjects]
    public class MvxNetworkDataStreamDefinitionEditor : Editor
    {
        private readonly GUIContent m_commandsSocketGuiContent = new GUIContent("Commands socket", "E.g. 'tcp://192.168.1.1:5555'");
        private readonly GUIContent m_dataSocketGuiContent = new GUIContent("Data socket", "E.g. 'tcp://192.168.1.1:5556'");
        private readonly GUIContent m_receiveBufferCapacityGuiContent = new GUIContent("Receive buffer capacity");
        private readonly GUIContent m_responseReceiveTimeoutGuiContent = new GUIContent("Response receive timeout");

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "MvxNetworkDataStreamDefinition properties changed");

            DrawDefaultInspector();
            DrawCommandsSocketProperty();
            DrawDataSocketProperty();
            DrawReceiveBufferCapacityProperty();
            DrawResponseReceiveTimeoutProperty();
        }

        private void DrawCommandsSocketProperty()
        {
            string commandsSocket = ((MvxNetworkDataStreamDefinition)target).commandsSocket;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxNetworkDataStreamDefinition)targetObject).commandsSocket != commandsSocket;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            string newCommandsSocket = EditorGUILayout.TextField(m_commandsSocketGuiContent, commandsSocket);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (commandsSocket != newCommandsSocket)
            {
                foreach (object targetObject in targets)
                {
                    ((MvxNetworkDataStreamDefinition)targetObject).commandsSocket = newCommandsSocket;
                    EditorUtility.SetDirty(((MvxNetworkDataStreamDefinition)targetObject));
                }
            }
        }

        private void DrawDataSocketProperty()
        {
            string dataSocket = ((MvxNetworkDataStreamDefinition)target).dataSocket;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxNetworkDataStreamDefinition)targetObject).dataSocket != dataSocket;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            string newDataSocket = EditorGUILayout.TextField(m_dataSocketGuiContent, dataSocket);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (dataSocket != newDataSocket)
            {
                foreach (object targetObject in targets)
                    ((MvxNetworkDataStreamDefinition)targetObject).dataSocket = newDataSocket;
            }
        }

        private void DrawReceiveBufferCapacityProperty()
        {
            uint bufferCapacity = ((MvxNetworkDataStreamDefinition)target).receiveBufferCapacity;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxNetworkDataStreamDefinition)targetObject).receiveBufferCapacity != bufferCapacity;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            uint newBufferCapacity = (uint) Mathf.Max(0, EditorGUILayout.IntField(m_receiveBufferCapacityGuiContent, (int)bufferCapacity));
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (bufferCapacity != newBufferCapacity)
            {
                foreach (object targetObject in targets)
                    ((MvxNetworkDataStreamDefinition)targetObject).receiveBufferCapacity = newBufferCapacity;
            }
        }

        private void DrawResponseReceiveTimeoutProperty()
        {
            long responseReceiveTimeout = ((MvxNetworkDataStreamDefinition)target).responseReceiveTimeout;
            bool mixedValue = false;

            foreach (object targetObject in targets)
                mixedValue = mixedValue || ((MvxNetworkDataStreamDefinition)targetObject).responseReceiveTimeout != responseReceiveTimeout;

            bool originalShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedValue;
            long newResponseReceiveTimeout = EditorGUILayout.LongField(m_responseReceiveTimeoutGuiContent, responseReceiveTimeout);
            EditorGUI.showMixedValue = originalShowMixedValue;

            if (responseReceiveTimeout != newResponseReceiveTimeout)
            {
                foreach (object targetObject in targets)
                    ((MvxNetworkDataStreamDefinition)targetObject).responseReceiveTimeout = newResponseReceiveTimeout;
            }
        }
    }
}