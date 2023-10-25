using UnityEngine;

namespace MVXUnity.UI
{
    public class MvxDataStreamPauseResume : MonoBehaviour
    {
        [SerializeField] public MVXUnity.MvxDataStream dataStream;

        private bool m_paused = false;

        public void PauseResume()
        {
            m_paused = !m_paused;
            if (m_paused)
                dataStream.Pause();
            else
                dataStream.Resume();
        }
    }
}
