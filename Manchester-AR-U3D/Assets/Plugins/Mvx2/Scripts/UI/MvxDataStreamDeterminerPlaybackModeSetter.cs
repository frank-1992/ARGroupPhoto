using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MVXUnity.UI
{
    public class MvxDataStreamDeterminerPlaybackModeSetter : MonoBehaviour
    {
        [SerializeField] public MVXUnity.MvxDataStreamDeterminer dataStream;
        [SerializeField] public Dropdown playbackModeDropdown;

        private void Reset()
        {
            playbackModeDropdown = GetComponent<Dropdown>();
        }

        void Start()
        {
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            options.Add(new Dropdown.OptionData(Mvx2API.RunnerPlaybackMode.RPM_FORWARD_ONCE.ToString()));
            options.Add(new Dropdown.OptionData(Mvx2API.RunnerPlaybackMode.RPM_FORWARD_LOOP.ToString()));
            options.Add(new Dropdown.OptionData(Mvx2API.RunnerPlaybackMode.RPM_BACKWARD_ONCE.ToString()));
            options.Add(new Dropdown.OptionData(Mvx2API.RunnerPlaybackMode.RPM_BACKWARD_LOOP.ToString()));
            options.Add(new Dropdown.OptionData(Mvx2API.RunnerPlaybackMode.RPM_PINGPONG.ToString()));
            options.Add(new Dropdown.OptionData(Mvx2API.RunnerPlaybackMode.RPM_PINGPONG_INVERSE.ToString()));
            options.Add(new Dropdown.OptionData(Mvx2API.RunnerPlaybackMode.RPM_REALTIME.ToString()));
            playbackModeDropdown.options = options;

            int activeOption = -1;
            switch (dataStream.playbackMode)
            {
                case Mvx2API.RunnerPlaybackMode.RPM_FORWARD_ONCE: activeOption = 0; break;
                case Mvx2API.RunnerPlaybackMode.RPM_FORWARD_LOOP: activeOption = 1; break;
                case Mvx2API.RunnerPlaybackMode.RPM_BACKWARD_ONCE: activeOption = 2; break;
                case Mvx2API.RunnerPlaybackMode.RPM_BACKWARD_LOOP: activeOption = 3; break;
                case Mvx2API.RunnerPlaybackMode.RPM_PINGPONG: activeOption = 4; break;
                case Mvx2API.RunnerPlaybackMode.RPM_PINGPONG_INVERSE: activeOption = 5; break;
                case Mvx2API.RunnerPlaybackMode.RPM_REALTIME: activeOption = 6; break;
            }

            playbackModeDropdown.value = activeOption;
        }

        public void OnDropdownValueChanged(int index)
        {
            Mvx2API.RunnerPlaybackMode playbackMode = (Mvx2API.RunnerPlaybackMode) System.Enum.Parse(typeof(Mvx2API.RunnerPlaybackMode), playbackModeDropdown.options[index].text);
            dataStream.playbackMode = playbackMode;
        }
    }
}
