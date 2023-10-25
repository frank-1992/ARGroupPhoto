using UnityEngine;
using UnityEngine.UI;
using System;

namespace MVXUnity
{
    public class MvxFPSCounter : MonoBehaviour
    {
        [SerializeField] public int averageRange = 60;

        public int averageFPS = 0;
        public int minFPS = 0;
        public int maxFPS = 0;
        
        [System.NonSerialized] private long[] m_timestampsBuffer = null;
        [System.NonSerialized] private int m_fpsBufferIndex = 0;

        [System.NonSerialized] private long m_lastSnapTimestamp = 0;   // in milliseconds (1 tick ~ 100 nanoseconds, 10000 ticks ~ 1 millisecond)

        void Awake()
        {
            InitializeFPSBuffer();
            m_lastSnapTimestamp = DateTime.Now.Ticks / 10000; // precision in milliseconds is good enough
        }

        private void InitializeFPSBuffer()
        {
            averageRange = Mathf.Max(averageRange, 1);
            m_timestampsBuffer = new long[averageRange];
        }

        public void SnapFrame()
        {
            if (m_timestampsBuffer == null || m_timestampsBuffer.Length != averageRange)
                InitializeFPSBuffer();

            long currentSnapTimestamp = DateTime.Now.Ticks / 10000;
            long deltaMilliseconds = currentSnapTimestamp - m_lastSnapTimestamp;
            m_lastSnapTimestamp = currentSnapTimestamp;
            
            m_timestampsBuffer[m_fpsBufferIndex] = deltaMilliseconds;
            m_fpsBufferIndex = (m_fpsBufferIndex + 1) % averageRange;
        }

        protected virtual void Update()
        {
            CalculateFPS();
            UpdateFPSGUI();
        }

        private void CalculateFPS()
        {
            int sum = 0;
            minFPS = int.MaxValue;
            maxFPS = 0;

            int validFPSValuesCount = 0;
            for (int fpsIndex = 0; fpsIndex < averageRange; fpsIndex++)
            {
                int timestamp = (int)m_timestampsBuffer[fpsIndex];
                if (timestamp > 0)
                {
                    int fps = 1000 / timestamp;
                    sum += fps;
                    validFPSValuesCount++;
                    minFPS = Mathf.Min(minFPS, fps);
                    maxFPS = Mathf.Max(maxFPS, fps);
                }
            }

            if (validFPSValuesCount > 0)
                averageFPS = sum / validFPSValuesCount;
        }

        #region GUI
        
        public Text averageFPSText = null;
        public Text minFPSText = null;
        public Text maxFPSText = null;

        private void UpdateFPSGUI()
        {
            if (averageFPSText != null)
                averageFPSText.text = FPSToString(averageFPS);
            if (minFPSText != null)
                minFPSText.text = FPSToString(minFPS);
            if (maxFPSText != null)
                maxFPSText.text = FPSToString(maxFPS);
        }

        private string FPSToString(int fps)
        {
            fps = Mathf.Clamp(fps, 0, MvxFPSValues.fpsStrings.Length-1);
            return MvxFPSValues.fpsStrings[fps];
        }

        #endregion
    }
}
