#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System.Collections.Generic;
using System;

namespace HeathenEngineering.SteamApi.Networking
{
    [Serializable]
    public struct LobbyHunterFilter
    {
        public int maxResults;
        public bool useDistanceFilter;
        public ELobbyDistanceFilter distanceOption;
        public bool useSlotsAvailable;
        public int requiredOpenSlots;
        public List<LobbyHunterNearFilter> nearValues;
        public List<LobbyHunterNumericFilter> numberValues;
        public List<LobbyHunterStringFilter> stringValues;
    }
}
#endif