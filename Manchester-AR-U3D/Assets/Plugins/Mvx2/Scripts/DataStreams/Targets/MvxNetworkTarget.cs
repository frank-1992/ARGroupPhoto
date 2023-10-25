using UnityEngine;

namespace MVXUnity
{
    [AddComponentMenu("Mvx2/Targets/Network Target")]
    public class MvxNetworkTarget : MvxTarget
    {
        #region data

        [SerializeField, Tooltip("E.g. 'tcp://*:5555'")] public string commandsSocket = "tcp://*:5555";
        [SerializeField, Tooltip("E.g. 'tcp://*:5556'")] public string dataSocket = "tcp://*:5556";

        [SerializeField] public uint sendBufferCapacity = 2;

        #endregion

        #region graph targets

        public override Mvx2API.GraphNode GetGraphNode()
        {
            return new Mvx2BasicIO.NetworkTransmitterGraphNode(commandsSocket, dataSocket, sendBufferCapacity);
        }

        #endregion
    }
}
