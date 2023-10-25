using UnityEngine;
using UnityEngine.Events;

namespace MVXUnity
{
    [AddComponentMenu("Mvx2/Targets/Frame Access Target")]
    public class MvxFrameAccessTarget : MvxTarget
    {
        #region data

        [System.Serializable] public class NextFrameReceivedEvent : UnityEvent<MVCommon.SharedRef<Mvx2API.Frame>> { }
        [SerializeField] public NextFrameReceivedEvent onNextFrameReceived = new NextFrameReceivedEvent();

        private Mvx2API.AsyncFrameAccessGraphNode m_asyncFrameAccessGraphTarget = null;

        private void OnMvxFrame(Mvx2API.Frame frame)
        {
            onNextFrameReceived.Invoke(new MVCommon.SharedRef<Mvx2API.Frame>(frame));
        }

        #endregion

        #region graph targets

        public override Mvx2API.GraphNode GetGraphNode()
        {
            if (m_asyncFrameAccessGraphTarget == null)
            {
                m_asyncFrameAccessGraphTarget = new Mvx2API.AsyncFrameAccessGraphNode(new Mvx2API.DelegatedFrameListener(OnMvxFrame));
            }

            return m_asyncFrameAccessGraphTarget;
        }

        #endregion
    }
}
