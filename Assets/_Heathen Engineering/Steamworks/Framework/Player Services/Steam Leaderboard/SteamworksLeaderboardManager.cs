#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// Handles leaderboards within Steamworks
    /// </summary>
    public class SteamworksLeaderboardManager : MonoBehaviour
    {
        public List<SteamworksLeaderboardData> Leaderboards;
        public UnityLeaderboardRankChangeEvent LeaderboardRankChanged;
        public UnityLeaderboardRankUpdateEvent LeaderboardRankLoaded;
        public UnityLeaderboardRankChangeEvent LeaderboardNewHighRank;

        private static SteamworksLeaderboardManager Instance = null;

        private void Start()
        {
            if(Instance != null)
            {
                Debug.LogWarning("[SteamworksLeaderboardManager.Start] Detected a possible duplicate Steamworks Leaderboard Manager, this may cause unexpected behaviour.");
            }

            Instance = this;
        }

        private void OnEnable()
        {
            //Register the leaderboards
            foreach (var l in Leaderboards)
            {
                l.Register();
                l.UserRankChanged.AddListener(HandleLeaderboardRankChanged);
                l.UserRankLoaded.AddListener(HandleLeaderboardRankLoaded);
                l.UserNewHighRank.AddListener(HandleLeaderboardNewHighRank);
            }
        }

        private void HandleLeaderboardRankLoaded(LeaderboardUserData arg0)
        {
            LeaderboardRankLoaded.Invoke(arg0);
        }

        private void HandleLeaderboardRankChanged(LeaderboardRankChangeData arg0)
        {
            LeaderboardRankChanged.Invoke(arg0);
        }

        private void HandleLeaderboardNewHighRank(LeaderboardRankChangeData arg0)
        {
            LeaderboardNewHighRank.Invoke(arg0);
        }

        /// <summary>
        /// Get the leaderboard with the matching name if any
        /// </summary>
        /// <param name="name">The name of the leaderboard to find</param>
        /// <returns>Returns the leaderboard found if any else returns null</returns>
        public SteamworksLeaderboardData GetLeaderboard(string name)
        {
            return Leaderboards.FirstOrDefault(p => p.leaderboardName == name);
        }

        /// <summary>
        /// Get the leaderboard referenced by the change data
        /// </summary>
        /// <param name="chageData">A Leaderboard Rank Change Data object, most commonly had from Rank Change events</param>
        /// <returns>Returns the leaderboard found if any else returns null</returns>
        public SteamworksLeaderboardData GetLeaderboard(LeaderboardRankChangeData chageData)
        {
            return Leaderboards.FirstOrDefault(p => p.leaderboardName == chageData.leaderboardName);
        }

        #region Steam Leaderboard Wrapper
        /// <summary>
        /// Uploads a score change to the indicated leaderboard
        /// </summary>
        /// <param name="boardName">Name of the board to upload the score for</param>
        /// <param name="score">The score to upload</param>
        /// <param name="method">The upload method</param>
        public void UploadLeaderboardScore(string boardName, int score, ELeaderboardUploadScoreMethod method)
        {
            var l = Leaderboards.FirstOrDefault(p => p.leaderboardName == boardName);

            if (l != null)
            {
                l.UploadScore(score, method);
            }
            else
            {
                Debug.LogError("[SteamworksLeaderboardManager.UploadLeaderboardScore] Unable to locate leaderboard [" + boardName + "], make sure the board is referenced in the Steamworks Leaderboard Manager.");
            }
        }

        /// <summary>
        /// Uploads a score change to the indicated leaderboard
        /// </summary>
        /// <param name="boardIndex">Index of the board to upload the score for</param>
        /// <param name="score">The score to upload</param>
        /// <param name="method">The upload method</param>
        public void UploadLeaderboardScore(int boardIndex, int score, ELeaderboardUploadScoreMethod method)
        {
            if (boardIndex > 0 && boardIndex < Leaderboards.Count)
            {
                var l = Leaderboards[boardIndex];

                if (l != null)
                {
                    l.UploadScore(score, method);
                }
            }
            else
            {
                Debug.LogError("[SteamworksLeaderboardManager.UploadLeaderboardScore] boardIndex is out of bounds, the value must be greater than 0 and less than Leaderboards.Count");
            }
        }

        /// <summary>
        /// Uploads a score change to the indicated leaderboard
        /// </summary>
        /// <param name="leaderboard">The board to upload the score for</param>
        /// <param name="score">The score to upload</param>
        /// <param name="method">The upload method</param>
        public void UploadLeaderboardScore(SteamworksLeaderboardData leaderboard, int score, ELeaderboardUploadScoreMethod method)
        {

            if (leaderboard != null)
            {
                leaderboard.UploadScore(score, method);
            }
            else
            {
                Debug.LogError("[SteamworksLeaderboardManager.UploadLeaderboardScore] Leaderboard is null, no score will be uploaded.");
            }
        }
        #endregion

        #region Static Steam Leaderboard Wrapper
        /// <summary>
        /// Uploads a score change to the indicated leaderboard
        /// </summary>
        /// <param name="boardName">Name of the board to upload the score for</param>
        /// <param name="score">The score to upload</param>
        /// <param name="method">The upload method</param>
        public static void _UploadLeaderboardScore(string boardName, int score, ELeaderboardUploadScoreMethod method)
        {
            if (Instance == null)
                return;

            var l = Instance.Leaderboards.FirstOrDefault(p => p.leaderboardName == boardName);

            if (l != null)
            {
                l.UploadScore(score, method);
            }
            else
            {
                Debug.LogError("Unable to locate leaderboard [" + boardName + "], make sure the board is referenced in the Heathen Steam Manager.");
            }
        }

        /// <summary>
        /// Uploads a score change to the indicated leaderboard
        /// </summary>
        /// <param name="boardIndex">Index of the board to upload the score for</param>
        /// <param name="score">The score to upload</param>
        /// <param name="method">The upload method</param>
        public static void _UploadLeaderboardScore(int boardIndex, int score, ELeaderboardUploadScoreMethod method)
        {
            if (Instance == null)
                return;

            if (boardIndex > 0 && boardIndex < Instance.Leaderboards.Count)
            {
                var l = Instance.Leaderboards[boardIndex];

                if (l != null)
                {
                    l.UploadScore(score, method);
                }
            }
            else
            {
                Debug.LogError("boardIndex is out of bounds, the value must be greater than 0 and less than Leaderboards.Count");
            }
        }

        /// <summary>
        /// Uploads a score change to the indicated leaderboard
        /// </summary>
        /// <param name="leaderboard">The board to upload the score for</param>
        /// <param name="score">The score to upload</param>
        /// <param name="method">The upload method</param>
        public static void _UploadLeaderboardScore(SteamworksLeaderboardData leaderboard, int score, ELeaderboardUploadScoreMethod method)
        {
            if (Instance == null)
                return;

            Instance.UploadLeaderboardScore(leaderboard, score, method);
        }
        #endregion
    }
}
#endif