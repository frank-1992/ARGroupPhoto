using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MVXUnity
{
    public class MvxAudioPlayer
    {
        #region data

        private Queue<MvxAudioChunk> m_audioChunksQueue = new Queue<MvxAudioChunk>(32);

        public const int AUDIO_CHUNKS_QUEUE_CAPACITY_UNLIMITED = -1;
        private int m_audioChunksQueueCapacity = AUDIO_CHUNKS_QUEUE_CAPACITY_UNLIMITED;
        private bool audioChunksQueueCapacityUnlimited
        {
            get { return m_audioChunksQueueCapacity <= AUDIO_CHUNKS_QUEUE_CAPACITY_UNLIMITED; }
        }

        private MvxAudioChunk m_currentAudioChunk = null;
        private MvxAudioChunk currentAudioChunk
        {
            get { return m_currentAudioChunk; }
            set
            {
                if (m_currentAudioChunk != null)
                    onAudioChunkDiscarded.Invoke(m_currentAudioChunk);

                m_currentAudioChunk = value;
            }
        }
        private int m_currentAudioChunkIndex = 0;
        private int currentAudioChunkSampleRate
        {
            get { return currentAudioChunk == null ? 0 : (int)currentAudioChunk.sampleRate; }
        }
        private int currentAudioChunkChannelsCount
        {
            get { return currentAudioChunk == null ? 0 : (int)currentAudioChunk.channelsCount; }
        }
        private float[] currentAudioChunkData
        {
            get { return currentAudioChunk == null ? null : currentAudioChunk.data; }
        }
        private int currentAudioChunkDataSize
        {
            get { return currentAudioChunk == null ? 0 : currentAudioChunk.dataSize; }
        }

        /// <summary> Indicates how much filled is source sample rate by target sample rate during the resampling process. </summary>
        /// <remarks>
        /// When value is equal to 1, the target sample rate fully filled the source rate and the next source sample will be used as is.
        /// When value is greater than 1, the target sample rate even overshot the source rate, so the next source sample will most likely be lerped 
        /// with the subsequent sample.
        /// When value is smaller than 1, the target sample rate did not fill the source rate yet, so the current sample will be used again and lerped
        /// with the subsequent one, using the new ratio.
        /// </remarks>
        private float m_sourceRateByTargetRateFeed = 0f;

        /// <summary> Constructor. </summary>
        /// <param name="audioChunksQueueCapacity">
        /// If positive value provided, the internal audio chunks queue will have a limited capacity, so enqueueing
        /// additional audio chunks will cause removal of older audio chunks. Negative value is interpreted as unlimited queue capacity.
        /// </param>
        public MvxAudioPlayer(int audioChunksQueueCapacity = AUDIO_CHUNKS_QUEUE_CAPACITY_UNLIMITED)
        {
            m_audioChunksQueueCapacity = audioChunksQueueCapacity;
        }

        public class AudioChunkEvent : UnityEvent<MvxAudioChunk> { }
        public AudioChunkEvent onAudioChunkPlaybackStarted = new AudioChunkEvent();
        public AudioChunkEvent onAudioChunkDiscarded = new AudioChunkEvent();

        private void DequeueNextBufferedAudioChunk()
        {
            currentAudioChunk = m_audioChunksQueue.Dequeue();
            onAudioChunkPlaybackStarted.Invoke(currentAudioChunk);
        }

        #endregion

        #region audio enqueue

        public void EnqueueAudioChunk(MvxAudioChunk chunk)
        {
            lock (m_audioChunksQueue)
            {
                m_audioChunksQueue.Enqueue(chunk);

                // reset currently played audio chunk immediatelly in case the buffer is oversized
                if (AudioChunksQueueIsFull())
                {
                    ResetCurrentlyPlayedAudioChunk();

                    while (AudioChunksQueueIsFull())
                    {
                        MvxAudioChunk dequeuedAudioChunk = m_audioChunksQueue.Dequeue();
                        onAudioChunkDiscarded.Invoke(dequeuedAudioChunk);
                    }
                }
            }
        }

        private bool AudioChunksQueueIsFull()
        {
            if (audioChunksQueueCapacityUnlimited)
                return false;

            return m_audioChunksQueue.Count > m_audioChunksQueueCapacity;
        }

        #endregion

        #region audio dequeue

        /// <summary> Streams buffered audio chunks for its playback. </summary>
        /// <remarks> Performs resampling if necessary. </remarks>
        /// <param name="targetData"> an array to stream audio data to </param>
        /// <param name="targetChannelsCount"> a count of channels to stream for </param>
        /// <param name="targetSampleRate"> a target sample rate of the streamed audio data </param>
        public void DequeueAudioData(float[] targetData, int targetChannelsCount, int targetSampleRate)
        {
            if (targetSampleRate == 0 || targetChannelsCount == 0)
                return;

            lock (m_audioChunksQueue)
            {
                int targetDataIndex = 0;

                // indicates how many source samples are required for single target sample to be equal in time duration
                // conversion ratio > 1 => some source samples will be skipped and those used will be lerped
                // conversion ratio == 1 => all source samples will become output samples
                // conversion ratio < 1 => additional lerped source samples will be inserted in between original source samples and even those will be lerped
                float sampleRateConversionRatio;

                while (targetDataIndex < targetData.Length)
                {
                    if (currentAudioChunk == null)
                    {
                        if (m_audioChunksQueue.Count == 0)
                        {
                            FeedZerosToTargetData(targetData, targetDataIndex);
                            ResetCurrentlyPlayedAudioChunk();
                            return;
                        }
                        
                        DequeueNextBufferedAudioChunk();
                    }

                    sampleRateConversionRatio = (float)currentAudioChunkSampleRate / (float)targetSampleRate;

                    if (!FeedNextSourceSampleToTargetData(targetData, ref targetDataIndex, targetChannelsCount))
                    {
                        // no more audio samples (not even buffered audio chunks) -> fill the rest of the target data with zeros
                        FeedZerosToTargetData(targetData, targetDataIndex);
                        ResetCurrentlyPlayedAudioChunk();
                        return;
                    }

                    m_sourceRateByTargetRateFeed += sampleRateConversionRatio;

                    if (!SkipResampledAudioData(targetSampleRate, ref sampleRateConversionRatio))
                    {
                        // no more audio samples (not even buffered audio chunks) -> fill the rest of the target data with zeros
                        FeedZerosToTargetData(targetData, targetDataIndex);
                        ResetCurrentlyPlayedAudioChunk();
                        return;
                    }
                }
            }
        }

        private bool FeedNextSourceSampleToTargetData(float[] targetData, ref int targetDataStartIndex, int targetChannelsCount)
        {
            if (m_currentAudioChunkIndex + currentAudioChunkChannelsCount < currentAudioChunkDataSize)
            {
                FeedSampleFromSourceToTargetData(
                    currentAudioChunkData, m_currentAudioChunkIndex, currentAudioChunkChannelsCount,
                    targetData, targetDataStartIndex, targetChannelsCount,
                    m_sourceRateByTargetRateFeed);

                targetDataStartIndex += targetChannelsCount;
                return true;
            }

            // current audio chunk does not have enough audio data -> try next one
            if (m_audioChunksQueue.Count == 0)
                return false;

            // lerp with next buffered audio chunk
            MvxAudioChunk nextAudioChunk = m_audioChunksQueue.Peek();

            int concatenatedAudioChannelsCount;
            float[] concatenatedAudioData = PrepareAuxiliaryDataForAudioChunksConcat(
                currentAudioChunk, m_currentAudioChunkIndex,
                nextAudioChunk, out concatenatedAudioChannelsCount);
            
            FeedSampleFromSourceToTargetData(
                concatenatedAudioData, 0, concatenatedAudioChannelsCount,
                targetData, targetDataStartIndex, targetChannelsCount,
                m_sourceRateByTargetRateFeed);

            targetDataStartIndex += targetChannelsCount;
            return true;
        }

        private bool SkipResampledAudioData(float targetSampleRate, ref float sampleRateConversionRatio)
        {
            // skip source samples in case the feed of source sample rate by target sample rate is full (==1) or overshot (>1), as these
            // are not needed in further resampling calculations
            while (m_sourceRateByTargetRateFeed >= 1f)
            {
                m_sourceRateByTargetRateFeed -= 1f;
                m_currentAudioChunkIndex += currentAudioChunkChannelsCount;

                if (m_currentAudioChunkIndex < currentAudioChunkDataSize)
                    continue;

                // switch to next buffered audio chunk if there is one
                if (m_audioChunksQueue.Count <= 0)
                    return false;

                m_currentAudioChunkIndex -= currentAudioChunkDataSize; // m_currentAudioChunkIndex >= currentAudioChunkDataSize

                DequeueNextBufferedAudioChunk();
                // recalculate sample rate conversion ratio for the new audio chunk
                sampleRateConversionRatio = (float)currentAudioChunkSampleRate / (float)targetSampleRate;
            }

            return true;
        }

        private static void FeedSampleFromSourceToTargetData(
            float[] sourceData, int sourceDataStartIndex, int sourceChannelsCount,
            float[] targetData, int targetDataStartIndex, int targetChannelsCount,
            float sourceRateByTargetRateFeed)
        {
            for (int targetChannelIndex = 0; targetChannelIndex < targetChannelsCount; targetChannelIndex++)
            {
                if (targetChannelIndex < sourceChannelsCount)
                {
                    // linear interpolation-based resampling
                    targetData[targetDataStartIndex + targetChannelIndex] = Mathf.Lerp(
                        (float)sourceData[sourceDataStartIndex + targetChannelIndex],
                        (float)sourceData[sourceDataStartIndex + targetChannelIndex + sourceChannelsCount],
                        sourceRateByTargetRateFeed);
                }
                else
                {
                    // fill remaining target channels with zero
                    targetData[targetDataStartIndex + targetChannelIndex] = 0f;
                }
            }
        }

        private static void FeedZerosToTargetData(float[] targetData, int targetDataStartIndex)
        {
            for (; targetDataStartIndex < targetData.Length; targetDataStartIndex++)
                targetData[targetDataStartIndex] = 0f;
        }

        private float[] m_auxiliaryAudioData = null;
        private float[] PrepareAuxiliaryDataForAudioChunksConcat(
            MvxAudioChunk precedingAudioChunk, int precedingAudioChunkIndex,
            MvxAudioChunk subsequentAudioChunk,
            out int channelsCount)
        {
            channelsCount = Mathf.Min((int)precedingAudioChunk.channelsCount, (int)subsequentAudioChunk.channelsCount);

            if (m_auxiliaryAudioData == null || m_auxiliaryAudioData.Length < channelsCount * 2)
                m_auxiliaryAudioData = new float[channelsCount * 2];

            for (int channelIndex = 0; channelIndex < channelsCount; channelIndex++)
            {
                m_auxiliaryAudioData[0 + channelIndex] = precedingAudioChunk.data[precedingAudioChunkIndex + channelIndex];
                m_auxiliaryAudioData[channelsCount + channelIndex] = subsequentAudioChunk.data[0 + channelIndex];
            }

            return m_auxiliaryAudioData;
        }

        public void Reset()
        {
            lock (m_audioChunksQueue)
            {
                while (m_audioChunksQueue.Count > 0)
                {
                    MvxAudioChunk dequeuedAudioChunk = m_audioChunksQueue.Dequeue();
                    onAudioChunkDiscarded.Invoke(dequeuedAudioChunk);
                }
                ResetCurrentlyPlayedAudioChunk();
            }
        }

        private void ResetCurrentlyPlayedAudioChunk()
        {
            currentAudioChunk = null;
            m_currentAudioChunkIndex = 0;
            m_sourceRateByTargetRateFeed = 0f;
        }
        
        /// <summary> Calculates queued audio data duration (in seconds) using given sample rate. </summary>
        /// <param name="sampleRate"> a sample rate to calculate the queued duration with </param>
        /// <returns> queued audio data duration </returns>
        public float GetQueuedAudioDuration(int sampleRate)
        {
            lock(m_audioChunksQueue)
            {
                float queuedDuration = 0f;
                foreach (MvxAudioChunk audioChunk in m_audioChunksQueue)
                    queuedDuration += (float)audioChunk.dataSize / (float)(audioChunk.channelsCount * sampleRate);

                // add also remaining duration of currently played audio chunk
                if (currentAudioChunk != null)
                    queuedDuration += (float)(currentAudioChunkDataSize - m_currentAudioChunkIndex) / (float)(currentAudioChunkChannelsCount * currentAudioChunkSampleRate);

                return queuedDuration;
            }
        }

        #endregion
    }
}
