#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.GameServices
{
    [Serializable]
    public class UnityWorkshopStartPlaytimeTrackingResultEvent : UnityEvent<StartPlaytimeTrackingResult_t>
    { }
}
#endif
