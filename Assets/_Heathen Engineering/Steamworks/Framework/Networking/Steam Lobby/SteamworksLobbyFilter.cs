#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;

namespace HeathenEngineering.SteamApi.Networking
{
    [Serializable]
    public struct SteamworksLobbyFilter
    {
        public string key;
        public string value;
        public ELobbyComparison method;
    }
}
#endif
