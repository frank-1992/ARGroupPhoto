using System;
using System.Collections.Generic;

namespace MVXUnity
{
    public class MvxAudioChunksPool
    {
        private MvxAudioChunksPool() { }

        private static MvxAudioChunksPool s_instance = new MvxAudioChunksPool();
        public static MvxAudioChunksPool instance
        {
            get
            {
                return s_instance;
            }
        }

        #region pool

        private Object m_threadLock = new object();

        private Queue<MvxAudioChunk> m_unusedAudioChunks = new Queue<MVXUnity.MvxAudioChunk>();
        private HashSet<MvxAudioChunk> m_usedAudioChunks = new HashSet<MVXUnity.MvxAudioChunk>();

        public MvxAudioChunk AllocateAudioChunk(float[] data, UInt32 channelsCount, UInt32 sampleRate)
        {
            lock(m_threadLock)
            {
                MvxAudioChunk audioChunk = null;

                if (m_unusedAudioChunks.Count > 0)
                {
                    audioChunk = m_unusedAudioChunks.Dequeue();
                    audioChunk.Reset(data, channelsCount, sampleRate);
                }
                else
                {
                    audioChunk = new MvxAudioChunk(data, channelsCount, sampleRate);
                }

                m_usedAudioChunks.Add(audioChunk);
                return audioChunk;
            }
        }

        public MvxAudioChunk AllocateAudioChunk(byte[] data, UInt32 bytesPerSample, UInt32 channelsCount, UInt32 sampleRate)
        {
            lock (m_threadLock)
            {
                MvxAudioChunk audioChunk = null;

                if (m_unusedAudioChunks.Count > 0)
                {
                    audioChunk = m_unusedAudioChunks.Dequeue();
                    audioChunk.Reset(data, bytesPerSample, channelsCount, sampleRate);
                }
                else
                {
                    audioChunk = new MvxAudioChunk(data, bytesPerSample, channelsCount, sampleRate);
                }

                m_usedAudioChunks.Add(audioChunk);
                return audioChunk;
            }
        }

        public void ReturnAudioChunk(MvxAudioChunk audioChunk)
        {
            lock(m_threadLock)
            {
                m_usedAudioChunks.Remove(audioChunk);
                m_unusedAudioChunks.Enqueue(audioChunk);
            }
        }

        #endregion
    }
}
