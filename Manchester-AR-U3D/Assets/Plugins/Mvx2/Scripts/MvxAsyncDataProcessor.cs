using UnityEngine;
using System.Threading;

namespace MVXUnity
{
    public abstract class MvxAsyncDataProcessor : MonoBehaviour
    {
		#region events

		public System.Action onProcessNextFrame;

		#endregion

		#region data 

		[SerializeField, HideInInspector] private MvxDataStream m_mvxStream = null;
        public MvxDataStream mvxStream
        {
            get { return m_mvxStream; }
            set
            {
                if (m_mvxStream == value)
                    return;

                if (isActiveAndEnabled)
                    DetachFromMvxStream();

                m_mvxStream = value;

                if (isActiveAndEnabled)
                    AttachToMvxStream();
            }
        }

        private bool m_canProcessStream = false;
        
        private object m_frameLock = new object();
        /// <summary> A temporary storage for asynchronously received frames - they can only be processed in the next Update() execution. </summary>
        private MVCommon.SharedRef<Mvx2API.Frame> m_frameToProcess = null;
        private MVCommon.SharedRef<Mvx2API.Frame> frameToProcess
        {
            get { return m_frameToProcess; }
            set
            {
                if (m_frameToProcess != null)
                    m_frameToProcess.Dispose();

                m_frameToProcess = value;
            }
        }

        #endregion

        #region stream

        private void AttachToMvxStream()
        {
            if (m_mvxStream == null)
                return;

            m_mvxStream.onStreamOpen.AddListener(HandleStreamOpen);
            m_mvxStream.onNextFrameReceived.AddListener(HandleNextFrameReceived);

            if (m_mvxStream.isOpen)
            {
                HandleStreamOpen(m_mvxStream.mvxSourceInfo);
                HandleNextFrameReceived(m_mvxStream.lastReceivedFrame);
            }
        }

        private void DetachFromMvxStream()
        {
            if (m_mvxStream == null)
                return;

            m_mvxStream.onStreamOpen.RemoveListener(HandleStreamOpen);
            m_mvxStream.onNextFrameReceived.RemoveListener(HandleNextFrameReceived);
        }

        #endregion

        #region stream handlers

        private void HandleStreamOpen(Mvx2API.SourceInfo sourceInfo)
        {
            frameToProcess = null;
            m_canProcessStream = CanProcessStream(sourceInfo);
            ResetProcessedData();

            if (mvxStream.lastReceivedFrame != null)
                HandleNextFrameReceived(mvxStream.lastReceivedFrame);
        }

        private void HandleNextFrameReceived(MVCommon.SharedRef<Mvx2API.Frame> frame)
        {
            if (!m_canProcessStream)
                return;

            if (frame == null)
                return;

            MVCommon.SharedRef<Mvx2API.Frame> newFrame = frame.CloneRef();

            // do not process the frame in this thread, instead store it and wait for the next Update()
            lock (m_frameLock)
                frameToProcess = newFrame;
        }

        // Check the stream info for data layers present in the stream
        protected abstract bool CanProcessStream(Mvx2API.SourceInfo sourceInfo);

        /// <summary> Resets data processor's already processed data. </summary>
        protected abstract void ResetProcessedData();

        protected abstract void ProcessNextFrame(MVCommon.SharedRef<Mvx2API.Frame> frame);

        #endregion

        #region MonoBehaviour

        public virtual void Reset()
        {
            mvxStream = GetComponent<MvxDataStream>();
        }

        public virtual void Start()
        {
			DetachFromMvxStream();
            AttachToMvxStream();
        }

        public virtual void OnDestroy()
        {
            DetachFromMvxStream();
        }

        public virtual void Update()
        {
            if (!Monitor.TryEnter(m_frameLock))
                return; // do not process the frame in case another frame is being received - waiting in Update() is unacceptable

            if (frameToProcess != null)
            {
                if (frameToProcess.sharedObj != null)
                    ProcessNextFrame(frameToProcess);
                frameToProcess = null;

				if(onProcessNextFrame != null){
					onProcessNextFrame();
				}
            }

            Monitor.Exit(m_frameLock);
        }

        #endregion
    }
}
