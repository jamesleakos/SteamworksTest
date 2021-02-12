#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.Tools;
using Steamworks;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// Base class used by HeathenSteamLeaderboard to represent leaderboard entries
    /// Derive from this class and override the ApplyEntry method to create a custom entry record
    /// </summary>
    public class HeathenSteamLeaderboardEntry : HeathenUIBehaviour
    {
        public virtual void ApplyEntry(ExtendedLeaderboardEntry entry)
        { }
    }
}
#endif
