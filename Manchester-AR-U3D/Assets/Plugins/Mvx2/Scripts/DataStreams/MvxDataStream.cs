using UnityEngine;
using UnityEngine.Events;

namespace MVXUnity
{
    public abstract class MvxDataStream : MonoBehaviour
    {
        #region data

        // added by One Hamsa
        public virtual float playbackTime
        {
            get
            {
                float FPS = mvxSourceInfo.GetFPS();
                if (FPS == 0f)
                    FPS = 15f;
                return (float)lastReceivedFrameNr / FPS;
            }
        }

        [SerializeField, HideInInspector] private MvxDataStreamDefinition m_dataStreamDefinition;
        public virtual MvxDataStreamDefinition dataStreamDefinition
        {
            get { return m_dataStreamDefinition; }
            set
            {
                if (m_dataStreamDefinition == value)
                    return;

                if (m_dataStreamDefinition != null)
                    m_dataStreamDefinition.onDefinitionChanged.RemoveListener(TryRestartStream);

                m_dataStreamDefinition = value;

                if (m_dataStreamDefinition != null)
                    m_dataStreamDefinition.onDefinitionChanged.AddListener(TryRestartStream);

                if (Application.isPlaying && isActiveAndEnabled)
                    RestartStream();
            }
        }

        [SerializeField] public MvxTarget[] additionalTargets = null;
        protected void AddAdditionalGraphTargetsToGraph(Mvx2API.ManualGraphBuilder graphBuilder)
        {
            if (additionalTargets == null)
                return;

            foreach (MvxTarget additionalTarget in additionalTargets)
                graphBuilder = graphBuilder + additionalTarget.GetGraphNode();
        }

        public abstract Mvx2API.SourceInfo mvxSourceInfo
        {
            get;
        }

        public abstract bool isSingleFrameSource
        {
            get;
        }

        // added by One Hamsa
        public abstract bool isPlaying { get; }

        public abstract bool isOpen
        {
            get;
            // protected set; // added by One Hamsa
        }

        // added by One Hamsa
        public abstract bool isError
        {
            get;
            protected set;
        }

        // added by One Hamsa
        public uint lastReceivedFrameNr = 0;
        // public uint numFramesInSource = 0;

        private MVCommon.SharedRef<Mvx2API.Frame> m_lastReceivedFrame;
        public MVCommon.SharedRef<Mvx2API.Frame> lastReceivedFrame
        {
            get { return m_lastReceivedFrame; }
            protected set
            {
                if (m_lastReceivedFrame != null)
                    m_lastReceivedFrame.Dispose();

                m_lastReceivedFrame = value;

                // added by One Hamsa
                if (value != null)
                    lastReceivedFrameNr = m_lastReceivedFrame.sharedObj.GetStreamAtomNr();
            }
        }

        #endregion

        #region frames handling

        [System.Serializable] public class StreamOpenEvent : UnityEvent<Mvx2API.SourceInfo> { }
        [SerializeField, HideInInspector] public StreamOpenEvent onStreamOpen = new StreamOpenEvent();
        
        [System.Serializable] public class NextFrameReceivedEvent : UnityEvent<MVCommon.SharedRef<Mvx2API.Frame>> { }
        [SerializeField, HideInInspector] public NextFrameReceivedEvent onNextFrameReceived = new NextFrameReceivedEvent();

        #endregion

        #region stream

        private void TryRestartStream()
        {
            if (Application.isPlaying && isActiveAndEnabled)
                RestartStream();
        }

        public void RestartStream()
        {
            DisposeStream();
            InitializeStream();
        }

        public abstract void InitializeStream();
        public abstract void DisposeStream();

        public abstract void SeekFrame(uint frameID);

        public abstract void Pause();
        public abstract void Resume();

        #endregion

        #region MonoBehaviour

        public virtual void Awake()
        {
            MvxPluginsLoader.LoadPlugins();
        }

        public virtual void Start()
        {
            if (m_dataStreamDefinition != null)
                m_dataStreamDefinition.onDefinitionChanged.AddListener(RestartStream);

            InitializeStream();
        }

        public virtual void OnDestroy()
        {
            DisposeStream();
        }

        public virtual void Update()
        {
            if (!isOpen)
                return;

            if (isSingleFrameSource && lastReceivedFrame != null)
                DisposeStream();
        }

        #endregion
    }
}