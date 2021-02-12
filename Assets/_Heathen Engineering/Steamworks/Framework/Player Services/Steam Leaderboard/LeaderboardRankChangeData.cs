#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    public struct LeaderboardRankChangeData
    {
        public string leaderboardName;
        public SteamLeaderboard_t leaderboardId;
        public LeaderboardEntry_t? oldEntry;
        public LeaderboardEntry_t newEntry;
        public int rankDelta
        {
            get
            {
                if (oldEntry.HasValue)
                    return newEntry.m_nGlobalRank - oldEntry.Value.m_nGlobalRank;
                else
                    return newEntry.m_nGlobalRank;
            }
        }

        public int scoreDeta
        {
            get
            {
                if (oldEntry.HasValue)
                    return newEntry.m_nScore - oldEntry.Value.m_nScore;
                else
                    return newEntry.m_nScore;
            }
        }
    }
}
#endif