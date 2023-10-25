using UnityEngine;
using UnityEngine.UI;

namespace MVXUnity.UI
{
    public class MvxDataStreamFrameSeeker : MonoBehaviour
    {
        [SerializeField] public MVXUnity.MvxDataStream dataStream;
        [SerializeField] public Slider frameIDSlider;
        [SerializeField] public Text valueLabel;

        public void Update()
        {
            frameIDSlider.minValue = 0;
            frameIDSlider.maxValue = !dataStream.isOpen ? 0 : (int)dataStream.mvxSourceInfo.GetNumFrames() - 1;

            if (valueLabel != null)
                valueLabel.text = ((int)frameIDSlider.value).ToString();
        }

        public void SeekFrame()
        {
            dataStream.SeekFrame((uint)frameIDSlider.value);
        }
    }
}
