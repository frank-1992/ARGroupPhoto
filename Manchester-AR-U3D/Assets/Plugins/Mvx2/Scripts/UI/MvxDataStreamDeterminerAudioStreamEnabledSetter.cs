using UnityEngine;
using UnityEngine.UI;

namespace MVXUnity.UI
{
    public class MvxDataStreamDeterminerAudioStreamEnabledSetter : MonoBehaviour
    {
        [SerializeField] public MVXUnity.MvxDataStreamDeterminer dataStream;
        [SerializeField] public Toggle audioStreamEnabledToggle;

        void Reset()
        {
            audioStreamEnabledToggle = GetComponent<Toggle>();
        }

        void Start()
        {
            audioStreamEnabledToggle.isOn = dataStream.audioStreamEnabled;
        }

        public void OnToggleValueChanged(bool newValue)
        {
            dataStream.audioStreamEnabled = newValue;
        }
    }
}
