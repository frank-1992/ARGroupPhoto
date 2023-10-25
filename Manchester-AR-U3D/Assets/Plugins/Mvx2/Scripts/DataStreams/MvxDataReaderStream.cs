using UnityEngine;

namespace MVXUnity
{
    public abstract class MvxDataReaderStream : MvxDataStream
    {
        #region data

        private Mvx2API.SourceInfo m_mvxSourceInfo = null;
        public override Mvx2API.SourceInfo mvxSourceInfo
        {
            get { return m_mvxSourceInfo; }
        }

        private bool m_isSingleFrameSource = false;
        public override bool isSingleFrameSource
        {
            get { return m_isSingleFrameSource; }
        }

        private bool m_isOpen = false;
        public override bool isOpen
        {
            get { return m_isOpen; }
        }

        #endregion

        #region stream

        public override void InitializeStream()
        {
            if (isOpen)
                return;

            if (dataStreamDefinition == null)
            {
                Debug.LogWarning("Mvx2: Missing data stream definition");
                return;
            }

            if (!OpenReader())
            {
                DisposeReader();
                return;
            }

            if (!ExtractSourceInfo() || !SupportsSourceStream(m_mvxSourceInfo))
            {
                DisposeReader();
                DisposeSourceInfo();
                return;
            }

            m_isOpen = true;
            onStreamOpen.Invoke(m_mvxSourceInfo);
        }

        public override void DisposeStream()
        {
            if (!isOpen)
                return;

            DisposeReader();
            DisposeSourceInfo();

            m_isOpen = false;
        }

		//OneHamsa::Addition
		protected bool m_isError = false;
		public override bool isError {
			get { return m_isError; }
			protected set { m_isError = value; }
		}


		#endregion

		#region source info

		private bool ExtractSourceInfo()
        {
            Debug.Log("Mvx2: Extracting source info from source");

            m_mvxSourceInfo = mvxRunner.GetSourceInfo();
            if (m_mvxSourceInfo == null)
                return false;

            m_isSingleFrameSource = m_mvxSourceInfo.GetNumFrames() == 1;
            return true;
        }

        private void DisposeSourceInfo()
        {
            if (m_mvxSourceInfo != null)
            {
                m_mvxSourceInfo.Dispose();
                m_mvxSourceInfo = null;
            }
        }

        protected virtual bool SupportsSourceStream(Mvx2API.SourceInfo mvxSourceInfo)
        {
            return true;
        }

        #endregion

        #region reader

        protected abstract bool OpenReader();
        protected abstract void DisposeReader();
        protected abstract Mvx2API.GraphRunner mvxRunner
        {
            get;
        }

        #endregion
    }
}