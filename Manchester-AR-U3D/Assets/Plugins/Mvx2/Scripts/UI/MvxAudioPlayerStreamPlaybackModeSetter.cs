using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MVXUnity.UI
{
    public class MvxAudioPlayerStreamPlaybackModeSetter : MonoBehaviour
    {
        [SerializeField] public MVXUnity.MvxAudioPlayerStream dataStream;
        [SerializeField] public Dropdown playbackModeDropdown;

        private void Reset()
        {
            playbackModeDropdown = GetComponent<Dropdown>();
        }

        void Start()
        {
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            options.Add(new Dropdown.OptionData(MVXUnity.MvxAudioPlayerStream.AudioPlaybackMode.PLAYBACKMODE_FORWARD_ONCE.ToString()));
            options.Add(new Dropdown.OptionData(MVXUnity.MvxAudioPlayerStream.AudioPlaybackMode.PLAYBACKMODE_FORWARD_LOOP.ToString()));
            options.Add(new Dropdown.OptionData(MVXUnity.MvxAudioPlayerStream.AudioPlaybackMode.PLAYBACKMODE_REALTIME.ToString()));
            playbackModeDropdown.options = options;

            int activeOption = -1;
            switch (dataStream.playbackMode)
            {
                case MVXUnity.MvxAudioPlayerStream.AudioPlaybackMode.PLAYBACKMODE_FORWARD_ONCE: activeOption = 0; break;
                case MVXUnity.MvxAudioPlayerStream.AudioPlaybackMode.PLAYBACKMODE_FORWARD_LOOP: activeOption = 1; break;
                case MVXUnity.MvxAudioPlayerStream.AudioPlaybackMode.PLAYBACKMODE_REALTIME: activeOption = 2; break;
            }

            playbackModeDropdown.value = activeOption;
        }

        public void OnDropdownValueChanged(int index)
        {
            MVXUnity.MvxAudioPlayerStream.AudioPlaybackMode playbackMode = 
                (MVXUnity.MvxAudioPlayerStream.AudioPlaybackMode)
                System.Enum.Parse(typeof(MVXUnity.MvxAudioPlayerStream.AudioPlaybackMode), playbackModeDropdown.options[index].text);
            dataStream.playbackMode = playbackMode;
        }
    }
}
