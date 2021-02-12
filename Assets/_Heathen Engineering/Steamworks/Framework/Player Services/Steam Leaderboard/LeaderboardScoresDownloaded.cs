#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    [Serializable]
    public struct LeaderboardScoresDownloaded
    {
        public bool bIOFailure;
        public bool playerIncluded;
        public LeaderboardScoresDownloaded_t scoreData;
    }
}
#endif
