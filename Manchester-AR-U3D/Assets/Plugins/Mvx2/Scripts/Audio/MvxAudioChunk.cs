using System;

namespace MVXUnity
{
    /// <summary> A chunk of audio data in standard Windows PCM format. </summary>
    /// <remarks> 1-byte per sample -> unsigned; 2-bytes per sample -> signed, 4-bytes per sample -> signed</remarks>
    public class MvxAudioChunk
    {
        private float[] m_data;
        public float[] data
        {
            get { return m_data; }
        }
        /// <summary> Indicates true size of the audio chunk's data, since the buffer may have a bigger capacity. </summary>
        private int m_dataSize;
        public int dataSize
        {
            get { return m_dataSize; }
        }
        private UInt32 m_channelsCount;
        public UInt32 channelsCount
        {
            get { return m_channelsCount; }
        }
        private UInt32 m_sampleRate;
        public UInt32 sampleRate
        {
            get { return m_sampleRate; }
        }

        public MvxAudioChunk(float[] data, UInt32 channelsCount, UInt32 sampleRate)
        {
            m_data = data;
            m_dataSize = this.data.Length;
            m_channelsCount = channelsCount;
            m_sampleRate = sampleRate;
        }

        public MvxAudioChunk(byte[] data, UInt32 bytesPerSample, UInt32 channelsCount, UInt32 sampleRate)
        {
            m_data = ConvertAudioDataToFloats(data, bytesPerSample);
            m_dataSize = this.data.Length;
            m_channelsCount = channelsCount;
            m_sampleRate = sampleRate;
        }

        public void Reset(float[] newData, UInt32 newChannelsCount, UInt32 newSampleRate)
        {
            MvxUtils.EnsureCollectionMinimalCapacity(ref m_data, (UInt32)newData.Length);
            Array.Copy(newData, m_data, newData.Length);
            m_dataSize = newData.Length;
            m_channelsCount = newChannelsCount;
            m_sampleRate = newSampleRate;
        }

        public void Reset(byte[] newData, UInt32 bytesPerSample, UInt32 newChannelsCount, UInt32 newSampleRate)
        {
            m_dataSize = ConvertAudioDataToFloats(newData, ref m_data, bytesPerSample);
            m_channelsCount = newChannelsCount;
            m_sampleRate = newSampleRate;
        }

        #region conversions

        private static float[] ConvertAudioDataToFloats(byte[] byteData, UInt32 bytesPerSample)
        {
            switch (bytesPerSample)
            {
                case 1:
                    return ConvertUnsignedByteDataToFloats(byteData);
                case 2:
                    return ConvertSignedShortDataToFloats(byteData);
                case 4:
                    return ConvertSignedFloatDataToFloats(byteData);
                default:
                    throw new System.ArgumentException("Unsupported 'audio bytes per sample' value. Must be 1, 2 or 4.");
            }
        }

        private static float[] ConvertUnsignedByteDataToFloats(byte[] byteData)
        {
            float[] floatData = new float[byteData.Length];

            for (int index = 0; index < floatData.Length; index++)
            {
                floatData[index] = (float)byteData[index] / (float)byte.MaxValue;
                // transform range from <0, 1> to <-1, 1> (see PCM format standard)
                floatData[index] = floatData[index] * 2f - 1f;
            }

            return floatData;
        }

        private static short[] shortAuxValue = new short[1];

        private static float[] ConvertSignedShortDataToFloats(byte[] byteData)
        {
            if (byteData.Length % 2 != 0)
                throw new System.ArgumentException("Bytes array does not contain valid count of bytes");

            float[] floatData = new float[byteData.Length / 2];

            for (int floatDataIndex = 0; floatDataIndex < floatData.Length; floatDataIndex++)
            {
                int byteDataIndex = floatDataIndex * 2;
                Buffer.BlockCopy(byteData, byteDataIndex, shortAuxValue, 0, 2);
                short shortValue = shortAuxValue[0];
                floatData[floatDataIndex] = (float)shortValue / (float)short.MaxValue;
            }

            return floatData;
        }

        private static float[] floatAuxValue = new float[1];

        private static float[] ConvertSignedFloatDataToFloats(byte[] byteData)
        {
            if (byteData.Length % 4 != 0)
                throw new System.ArgumentException("Bytes array does not contain valid count of bytes");

            float[] floatData = new float[byteData.Length / 4];

            for (int floatDataIndex = 0; floatDataIndex < floatData.Length; floatDataIndex++)
            {
                int byteDataIndex = floatDataIndex * 4;
                Buffer.BlockCopy(byteData, byteDataIndex, floatAuxValue, 0, 4);
                // value is supposed to be in range <-1, 1> already  (see PCM format standard)
                floatData[floatDataIndex] = floatAuxValue[0];
            }

            return floatData;
        }

        private static int ConvertAudioDataToFloats(byte[] byteData, ref float[] floatData, UInt32 bytesPerSample)
        {
            switch (bytesPerSample)
            {
                case 1:
                    return ConvertUnsignedByteDataToFloats(byteData, ref floatData);
                case 2:
                    return ConvertSignedShortDataToFloats(byteData, ref floatData);
                case 4:
                    return ConvertSignedFloatDataToFloats(byteData, ref floatData);
                default:
                    throw new System.ArgumentException("Unsupported 'audio bytes per sample' value. Must be 1, 2 or 4.");
            }
        }

        private static int ConvertUnsignedByteDataToFloats(byte[] byteData, ref float[] floatData)
        {
            MvxUtils.EnsureCollectionMinimalCapacity(ref floatData, (UInt32)byteData.Length);

            for (int index = 0; index < byteData.Length; index++)
            {
                floatData[index] = (float)byteData[index] / (float)byte.MaxValue;
                // transform range from <0, 1> to <-1, 1> (see PCM format standard)
                floatData[index] = floatData[index] * 2f - 1f;
            }

            return byteData.Length;
        }

        private static int ConvertSignedShortDataToFloats(byte[] byteData, ref float[] floatData)
        {
            if (byteData.Length % 2 != 0)
                throw new System.ArgumentException("Bytes array does not contain valid count of bytes");

            int floatDataLength = byteData.Length / 2;
            MvxUtils.EnsureCollectionMinimalCapacity(ref floatData, (UInt32)floatDataLength);

            for (int floatDataIndex = 0; floatDataIndex < floatDataLength; floatDataIndex++)
            {
                int byteDataIndex = floatDataIndex * 2;
                Buffer.BlockCopy(byteData, byteDataIndex, shortAuxValue, 0, 2);
                short shortValue = shortAuxValue[0];
                floatData[floatDataIndex] = (float)shortValue / (float)short.MaxValue;
            }

            return floatDataLength;
        }

        private static int ConvertSignedFloatDataToFloats(byte[] byteData, ref float[] floatData)
        {
            if (byteData.Length % 4 != 0)
                throw new System.ArgumentException("Bytes array does not contain valid count of bytes");

            int floatDataLength = byteData.Length / 4;
            MvxUtils.EnsureCollectionMinimalCapacity(ref floatData, (UInt32)floatDataLength);

            for (int floatDataIndex = 0; floatDataIndex < floatDataLength; floatDataIndex++)
            {
                int byteDataIndex = floatDataIndex * 4;
                Buffer.BlockCopy(byteData, byteDataIndex, floatAuxValue, 0, 4);
                // value is supposed to be in range <-1, 1> already  (see PCM format standard)
                floatData[floatDataIndex] = floatAuxValue[0];
            }

            return floatDataLength;
        }

        #endregion
    }
}
