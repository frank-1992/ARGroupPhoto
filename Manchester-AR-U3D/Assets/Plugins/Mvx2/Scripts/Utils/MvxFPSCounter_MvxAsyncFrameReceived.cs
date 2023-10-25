using UnityEngine;
using System.Threading;

namespace MVXUnity
{
    public class MvxFPSCounter_MvxAsyncFrameReceived : MvxFPSCounter
    {
        [SerializeField] private MvxDataStream m_dataStream = null;

        public void Start()
        {
            m_dataStream.onNextFrameReceived.AddListener(OnNextFrameReceived);
        }

        private object m_frameLock = new object();

        private void OnNextFrameReceived(MVCommon.SharedRef<Mvx2API.Frame> frame)
        {
            lock (m_frameLock)
            {
                if (frame != null)
                    SnapFrame();
            }
        }

        protected override void Update()
        {
            if (!Monitor.TryEnter(m_frameLock))
                return;

            base.Update();

            Monitor.Exit(m_frameLock);
        }
    }
}