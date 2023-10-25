using UnityEngine;

namespace MVXUnity
{
    [AddComponentMenu("Mvx2/Data Streams/Data Random-access Stream")]
    public class MvxDataRandomAccessStream : MvxDataReaderStream
    {
        #region data

        [SerializeField, HideInInspector] protected uint m_frameId = 0;
        public virtual uint frameId
        {
            get
            {
                return m_frameId;
            }
            set
            {
                if (m_frameId == value)
                    return;

                m_frameId = value;
                if (Application.isPlaying && isActiveAndEnabled)
                    ReadFrame();
            }
        }

        // added by One Hamsa
        private bool m_paused = false;

        #endregion

        #region stream

        // added by One Hamsa
        public override bool isPlaying { get { return isOpen && !m_paused; } }

        public override void InitializeStream()
        {
            base.InitializeStream();
            if (isOpen)
                ReadFrame();
        }

        #endregion

        #region reader

        [System.NonSerialized] private Mvx2API.RandomAccessGraphRunner m_mvxRunner = null;
        [System.NonSerialized] private Mvx2API.FrameAccessGraphNode m_frameAccess = null;
        protected override Mvx2API.GraphRunner mvxRunner
        {
            get { return m_mvxRunner; }
        }

        protected override bool OpenReader()
        {
            lastReceivedFrame = null;

            // added by One Hamsa
            m_paused = false;

            try
            {
                m_frameAccess = new Mvx2API.FrameAccessGraphNode();

                Mvx2API.ManualGraphBuilder graphBuilder = new Mvx2API.ManualGraphBuilder();
                graphBuilder = graphBuilder + dataStreamDefinition.GetSourceGraphNode() + new Mvx2API.AutoDecompressorGraphNode() + m_frameAccess;
                AddAdditionalGraphTargetsToGraph(graphBuilder);

                m_mvxRunner = new Mvx2API.RandomAccessGraphRunner(graphBuilder.CompileGraphAndReset());
                Debug.Log("Mvx2: The stream is open and playing");
                return true;
            }
            catch (System.Exception exception)
            {
                Debug.LogErrorFormat("Failed to create the graph: {0}", exception.Message);
                m_mvxRunner = null;
                return false;
            }
        }

        protected override void DisposeReader()
        {
            if (m_frameAccess != null)
            {
                m_frameAccess.Dispose();
                m_frameAccess = null;
            }

            if (m_mvxRunner == null)
                return;

            m_mvxRunner.Dispose();
            m_mvxRunner = null;

            // added by One Hamsa
            m_paused = false;
        }

        public override void SeekFrame(uint frameID)
        {
            if (!isOpen)
                return;

            this.frameId = frameID;
        }

        public override void Pause()
        {
            // added by One Hamsa
            m_paused = true;
        }

        public override void Resume()
        {
            // added by One Hamsa
            m_paused = false;
        }

        #endregion

        #region frames reading

        protected void ReadFrame()
        {
            if (!m_mvxRunner.ProcessFrame(frameId))
                return;

            lastReceivedFrame = new MVCommon.SharedRef<Mvx2API.Frame>(m_frameAccess.GetRecentProcessedFrame());
            if (lastReceivedFrame.sharedObj != null)
                onNextFrameReceived.Invoke(lastReceivedFrame);
        }

        #endregion
    }
}
