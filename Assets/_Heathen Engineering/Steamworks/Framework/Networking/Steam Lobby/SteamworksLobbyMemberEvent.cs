#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Networking
{
    [Serializable]
    public class SteamworksLobbyMemberEvent : UnityEvent<SteamworksLobbyMember>
    { }

    [Serializable]
    public class SteamworksSteamIDEvent : UnityEvent<CSteamID>
    { }
}
#endif