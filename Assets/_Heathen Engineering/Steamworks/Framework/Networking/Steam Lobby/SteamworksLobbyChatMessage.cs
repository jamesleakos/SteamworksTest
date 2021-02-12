#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation.UI;
using HeathenEngineering.Tools;
using System;
using UnityEngine.EventSystems;

namespace HeathenEngineering.SteamApi.Networking
{
    public class SteamworksLobbyChatMessage : HeathenUIBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public SteamUserIconButton PersonaButton;
        public UnityEngine.UI.Text Message;
        public DateTime timeStamp;
        public UnityEngine.UI.Text timeRecieved;
        public bool ShowStamp = true;
        public bool AllwaysShowStamp = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ShowStamp && !timeRecieved.gameObject.activeSelf)
                timeRecieved.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(!AllwaysShowStamp && timeRecieved.gameObject.activeSelf)
                timeRecieved.gameObject.SetActive(false);
        }
    }
}
#endif
