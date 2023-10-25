using UnityEngine;

namespace MVXUnity
{
    /// <summary>
    /// Switches between audio samplerate-based and simple framerate-based streamers according to properties of the streamed source and preferences set.
    /// </summary>
    /// <remarks>
    /// For sources containing audio data, the stream supporting audio playback is preferred unless the audio playback is forbidden via property.
    /// </remarks>
    [AddComponentMenu("Mvx2/Data Streams/Data Stream Determiner")]
    public class MvxDataStreamDeterminer : MvxDataStream
    {
        #region data

        // added by One Hamsa
        public override float playbackTime {  get { return isPlaying ? currentStream.playbackTime : 0f; } }
        public override bool isPlaying { get { return (currentStream != null) ? currentStream.isPlaying : false; } }

        public override Mvx2API.SourceInfo mvxSourceInfo
        {
            get
            {
                return currentStream == null ? null : currentStream.mvxSourceInfo;
            }
        }

        public override bool isSingleFrameSource
        {
            get
            {
                return currentStream == null ? false : currentStream.isSingleFrameSource;
            }
        }

        public override bool isOpen
        {
            get
            {
                return (m_audioStream != null && m_audioStream.isOpen) || (m_simpleStream != null && m_simpleStream.isOpen);
            }
        }

		//OneHamsa::Addition
		public override bool isError {
			get {
				return (m_audioStream != null && m_audioStream.isError) || (m_simpleStream != null && m_simpleStream.isError);
			}
			protected set { }
		}

		[SerializeField, HideInInspector] private Mvx2API.RunnerPlaybackMode m_playbackMode;
        public Mvx2API.RunnerPlaybackMode playbackMode
        {
            get { return m_playbackMode; }
            set
            {
                if (m_playbackMode == value)
                    return;
                m_playbackMode = value;

                if (m_audioStream != null)
                    m_audioStream.playbackMode = GetSupportedAudioPlaybackMode(playbackMode);
                if (m_simpleStream != null)
                    m_simpleStream.playbackMode = m_playbackMode;
            }
        }

        private static MvxAudioPlayerStream.AudioPlaybackMode GetSupportedAudioPlaybackMode(Mvx2API.RunnerPlaybackMode playbackMode)
        {
            if (playbackMode == Mvx2API.RunnerPlaybackMode.RPM_FORWARD_LOOP || playbackMode == Mvx2API.RunnerPlaybackMode.RPM_BACKWARD_LOOP)
                return MvxAudioPlayerStream.AudioPlaybackMode.PLAYBACKMODE_FORWARD_LOOP;
            if (playbackMode == Mvx2API.RunnerPlaybackMode.RPM_FORWARD_ONCE || playbackMode == Mvx2API.RunnerPlaybackMode.RPM_BACKWARD_ONCE)
                return MvxAudioPlayerStream.AudioPlaybackMode.PLAYBACKMODE_FORWARD_ONCE;
            if (playbackMode == Mvx2API.RunnerPlaybackMode.RPM_REALTIME)
                return MvxAudioPlayerStream.AudioPlaybackMode.PLAYBACKMODE_REALTIME;

            return MvxAudioPlayerStream.AudioPlaybackMode.PLAYBACKMODE_FORWARD_LOOP;
        }

        [SerializeField, HideInInspector] private bool m_followStreamFPS = true;
        public bool followStreamFPS
        {
            get { return m_followStreamFPS; }
            set
            {
                if (m_followStreamFPS == value)
                    return;
                m_followStreamFPS = value;

                if (m_simpleStream != null)
                    m_simpleStream.followStreamFPS = m_followStreamFPS;
            }
        }

        [SerializeField, HideInInspector] private float m_minimalBufferedAudioDuration = 1f;
        public float minimalBufferedAudioDuration
        {
            get { return m_minimalBufferedAudioDuration; }
            set
            {
                if (m_minimalBufferedAudioDuration == value)
                    return;
                m_minimalBufferedAudioDuration = value;

                if (m_audioStream != null)
                    m_audioStream.minimalBufferedAudioDuration = m_minimalBufferedAudioDuration;
            }
        }

        #endregion

        #region nested streams data

        [SerializeField, HideInInspector] private bool m_audioStreamEnabled = true;
        public bool audioStreamEnabled
        {
            get { return m_audioStreamEnabled; }
            set
            {
                if (m_audioStreamEnabled == value)
                    return;
                m_audioStreamEnabled = value;

                if (Application.isPlaying && isActiveAndEnabled && isOpen)
                    RestartStream();
            }
        }

        private MvxSimpleDataStream m_simpleStream = null;
        private MvxAudioPlayerStream m_audioStream = null;

        private MvxDataStream currentStream
        {
            get
            {
                if (m_audioStream != null && m_audioStream.isOpen)
                    return m_audioStream;

                if (m_simpleStream != null && m_simpleStream.isOpen)
                    return m_simpleStream;

                return null;
            }
        }

        #endregion

        #region events propagation

        private void OnNestedStreamOpenedStream(Mvx2API.SourceInfo mvxSourceInfo)
        {
            onStreamOpen.Invoke(mvxSourceInfo);
        }

        private void OnNestedStreamReceivedNextFrame(MVCommon.SharedRef<Mvx2API.Frame> nextFrame)
        {
            lastReceivedFrame = nextFrame;
            onNextFrameReceived.Invoke(nextFrame);
        }

        #endregion

        #region stream

        public override void InitializeStream()
        {
            if (isOpen)
                return;

            lastReceivedFrame = null;

            /*
            if (m_audioStreamEnabled)
            {
                Debug.Log("Mvx2: Audio stream enabled, will attempt to run the source with audio");
                m_audioStream.playbackMode = GetSupportedAudioPlaybackMode(playbackMode);
                m_audioStream.minimalBufferedAudioDuration = minimalBufferedAudioDuration;
                m_audioStream.additionalTargets = additionalTargets;
				m_audioStream.dataStreamDefinition = dataStreamDefinition;
                m_audioStream.InitializeStream();

                if (m_audioStream.isOpen)
                    return;
                else
                    Debug.Log("Mvx: Failed to run audio stream, simple frame-rate based stream will be used");
            }
            else
            {
                Debug.Log("Mvx2: Audio stream disabled, simple frame-rate based stream will be used");
            }
            */

            m_simpleStream.playbackMode = playbackMode;
            m_simpleStream.followStreamFPS = followStreamFPS;
            m_simpleStream.additionalTargets = additionalTargets;
			m_simpleStream.dataStreamDefinition = dataStreamDefinition;
            m_simpleStream.InitializeStream();
        }

        public override void DisposeStream()
        {
            if (m_audioStream != null)
                m_audioStream.DisposeStream();
            if (m_simpleStream != null)
                m_simpleStream.DisposeStream();
        }

        public override void SeekFrame(uint frameID)
        {
            if (m_audioStream != null)
                m_audioStream.SeekFrame(frameID);
            if (m_simpleStream != null)
                m_simpleStream.SeekFrame(frameID);
        }

        public override void Pause()
        {
            if (m_audioStream != null)
                m_audioStream.Pause();
            if (m_simpleStream != null)
                m_simpleStream.Pause();
        }

        public override void Resume()
        {
            if (m_audioStream != null)
                m_audioStream.Resume();
            if (m_simpleStream != null)
                m_simpleStream.Resume();
        }

        #endregion

        #region MonoBehaviour

        public override void Awake()
        {
            base.Awake();

            m_simpleStream = gameObject.AddComponent<MvxSimpleDataStream>();
            m_simpleStream.onStreamOpen.AddListener(OnNestedStreamOpenedStream);
            m_simpleStream.onNextFrameReceived.AddListener(OnNestedStreamReceivedNextFrame);
            /*
            m_audioStream = gameObject.AddComponent<MvxAudioPlayerStream>();
            m_audioStream.onStreamOpen.AddListener(OnNestedStreamOpenedStream);
            m_audioStream.onNextFrameReceived.AddListener(OnNestedStreamReceivedNextFrame);
            */
        }

        public override void OnDestroy()
        {
            if (m_simpleStream != null)
            {
                m_simpleStream.onStreamOpen.RemoveListener(OnNestedStreamOpenedStream);
                m_simpleStream.onNextFrameReceived.RemoveListener(OnNestedStreamReceivedNextFrame);
                Destroy(m_simpleStream);
                m_simpleStream = null;
            }

            if (m_audioStream != null)
            {
                m_audioStream.onStreamOpen.RemoveListener(OnNestedStreamOpenedStream);
                m_audioStream.onNextFrameReceived.RemoveListener(OnNestedStreamReceivedNextFrame);
                Destroy(m_audioStream);
                m_audioStream = null;
            }

            base.OnDestroy();
        }

        public void OnEnable()
        {
            m_simpleStream.enabled = true;
            //m_audioStream.enabled = true;
        }

        public void OnDisable()
        {
            m_simpleStream.enabled = false;
            //m_audioStream.enabled = false;
        }

        #endregion
    }
}
