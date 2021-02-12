#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.SteamApi.Foundation.UI;
using Steamworks;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    public class BasicLeaderboardEntry : HeathenSteamLeaderboardEntry
    {
        public UnityEngine.UI.Text rank;
        public SteamUserFullIcon avatar;
        public string formatString;
        public UnityEngine.UI.Text score;
        public LeaderboardEntry_t data;

        public override void ApplyEntry(ExtendedLeaderboardEntry entry)
        {
            data = entry.Base;
            var userData = SteamSettings.current.client.GetUserData(entry.Base.m_steamIDUser);
            avatar.LinkSteamUser(userData);
            if (!string.IsNullOrEmpty(formatString))
                score.text = entry.Base.m_nScore.ToString(formatString);
            else
                score.text = entry.Base.m_nScore.ToString();

            rank.text = entry.Base.m_nGlobalRank.ToString();
        }
    }
}
#endif
