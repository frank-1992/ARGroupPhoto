using UnityEngine;
using UnityEngine.UI;

namespace MVXUnity.UI
{
    public class MvxDataRandomAccessStreamFrameIdSetter : MonoBehaviour
    {
        [SerializeField] public MvxDataRandomAccessStream dataStream;
        [SerializeField] public Slider slider;
        [SerializeField] public Text valueLabel;
        
        public void SetDataSourceFrameId(float value)
        {
            dataStream.frameId = (uint)value;
        }

        public void Reset()
        {
            slider = GetComponent<Slider>();
        }

        public void Update()
        {
            slider.minValue = 0;
            slider.maxValue = !dataStream.isOpen ? 0 : (int)dataStream.mvxSourceInfo.GetNumFrames() - 1;

            if (valueLabel != null)
                valueLabel.text = (!dataStream.isOpen ? 0 : (int)dataStream.frameId).ToString();
        }
    }
}
