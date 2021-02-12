#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using UnityEngine;

namespace HeathenEngineering.SteamApi.Demo
{
    public class ToggleInvertActive : MonoBehaviour
    {
        public UnityEngine.UI.Toggle toggle;
        public GameObject icon;

        private void Start()
        {
            icon.SetActive(!toggle.isOn);
            toggle.onValueChanged.AddListener(handleChange);
        }

        private void handleChange(bool arg0)
        {
            icon.SetActive(!arg0);
        }
    }
}
#endif