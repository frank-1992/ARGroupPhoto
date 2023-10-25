using UnityEngine;
using UnityEngine.UI;

namespace MVXUnity.UI
{
    public class MvxSimpleDataStreamFollowFPSSetter : MonoBehaviour
    {
        [SerializeField] public MVXUnity.MvxSimpleDataStream dataStream;
        [SerializeField] public Toggle followFPSToggle;

        void Reset()
        {
            followFPSToggle = GetComponent<Toggle>();
        }

        void Start()
        {
            followFPSToggle.isOn = dataStream.followStreamFPS;
        }

        public void OnToggleValueChanged(bool newValue)
        {
            dataStream.followStreamFPS = newValue;
        }
    }
}
