#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.Tools;
using Steamworks;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    public class HeathenLeaderboardStatTool : HeathenBehaviour
    {
        [Header("Settings")]
        public SteamworksLeaderboardData LeaderboardObject;
        public SteamStatData StatObject;
        public ELeaderboardUploadScoreMethod UpdateMethod = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;

        [Header("Debug Tools")]
        public bool ShowDebug = false;
        public int StatValue;
        public int UserScore;
        public int UserRank;

        private void Update()
        {
            if(ShowDebug)
            {
                if (LeaderboardObject != null && StatObject != null)
                {
                    StatValue = StatObject.GetIntValue();
                    UserScore = LeaderboardObject.UserEntry.HasValue ? LeaderboardObject.UserEntry.Value.m_nScore : 0;
                    UserRank = LeaderboardObject.UserEntry.HasValue ? LeaderboardObject.UserEntry.Value.m_nGlobalRank : 0;
                }
            }
        }

        public void Submit()
        {
            if(LeaderboardObject != null && StatObject != null)
            {
                LeaderboardObject.UploadScore(StatObject.GetIntValue(), UpdateMethod);
            }
        }
    }
}
#endif