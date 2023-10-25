using UnityEngine;

namespace MVXUnity
{
    [CreateAssetMenu(fileName = "NetworkDataStreamDefinition", menuName = "Mvx2/Data Stream Definitions/Network Data Stream Definition")]
    public class MvxNetworkDataStreamDefinition : MvxDataStreamDefinition
    {
        #region data

        [SerializeField, HideInInspector] private string m_commandsSocket;
        public string commandsSocket
        {
            get { return m_commandsSocket; }
            set
            {
                if (m_commandsSocket == value)
                    return;
                m_commandsSocket = value;

                onDefinitionChanged.Invoke();
            }
        }

        [SerializeField, HideInInspector] private string m_dataSocket;
        public string dataSocket
        {
            get { return m_dataSocket; }
            set
            {
                if (m_dataSocket == value)
                    return;
                m_dataSocket = value;

                onDefinitionChanged.Invoke();
            }
        }

        [SerializeField, HideInInspector] private uint m_receiveBufferCapacity = 5;
        public uint receiveBufferCapacity
        {
            get { return m_receiveBufferCapacity; }
            set
            {
                if (m_receiveBufferCapacity == value)
                    return;
                m_receiveBufferCapacity = value;

                onDefinitionChanged.Invoke();
            }
        }

        [SerializeField, HideInInspector] private long m_responseReceiveTimeout = 3000;
        public long responseReceiveTimeout
        {
            get { return m_responseReceiveTimeout; }
            set
            {
                if (m_responseReceiveTimeout == value)
                    return;
                m_responseReceiveTimeout = value;

                onDefinitionChanged.Invoke();
            }
        }

        #endregion

        #region graph node

        public override Mvx2API.GraphNode GetSourceGraphNode()
        {
            return new Mvx2BasicIO.NetworkReceiverGraphNode(commandsSocket, dataSocket, receiveBufferCapacity, responseReceiveTimeout);
        }

        #endregion
    }
}
