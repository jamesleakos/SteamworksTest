#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using UnityEngine;

/// <summary>
/// Always put your code in a namespace and insist others do the same
/// This is not a matter of vanity, not using namespaces causes other peoples code to break ... feel free to ask me more about this via our Discord channel
/// </summary>
namespace HeathenEngineering.SteamApi.PlayerServices.Demo
{
    /// <summary>
    /// Demonstrates the use of <see cref="SteamworksLeaderboardData"/> objects to update user scores.
    /// </summary>
    public class ExampleLeaderboardScoring : MonoBehaviour
    {
        public SteamworksLeaderboardData leaderboardData;

        /// <summary>
        /// Simple keep best score update
        /// </summary>
        /// <param name="score"></param>
        public void UpdateScore(int score)
        {
            //This just sends the current score and lets Steam decide if this is better than the previous or not
            leaderboardData.UploadScore(score, Steamworks.ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest);
            Debug.Log("Set leaderboard: " + leaderboardData.leaderboardName + " score to: " + score.ToString() + " with instruction to keep the best value (comparing current vs new)");
        }

        /// <summary>
        /// Force a score update e.g. tell Steam to use this score value rather or not its better than the current value
        /// </summary>
        /// <param name="score"></param>
        public void ForceUpdateScore(int score)
        {
            //This sends the current score and tells Steam to overwrite the old score
            leaderboardData.UploadScore(score, Steamworks.ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate);
            Debug.Log("Set leaderboard: " + leaderboardData.leaderboardName + " score to: " + score.ToString() + " with instruction to overwrite the current value");
        }

        /// <summary>
        /// Add this amount to the current score value
        /// </summary>
        /// <param name="score"></param>
        public void AddToScore(int score)
        {
            //This gets whatever the last score was and adds the new score to it ... which is odd for a leaderboard but what you asked for
            int currentScore = leaderboardData.UserEntry.HasValue ? leaderboardData.UserEntry.Value.m_nScore : 0;
            leaderboardData.UploadScore(currentScore + score, Steamworks.ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest);
            Debug.Log("Set leaderboard: " + leaderboardData.leaderboardName + " score to: " + (currentScore + score).ToString() + " with instruction to keep the best value (comparing current vs new)");
        }

        /// <summary>
        /// Resets the user's stats and achievements ... useful for testing
        /// </summary>
        public void ResetScoreAndAchievements()
        {
            SteamUserStats.ResetAllStats(true);
        }

        /// <summary>
        /// Opens the Valve documentation to the Leaderboards page.
        /// </summary>
        public void GetHelp()
        {
            Application.OpenURL("https://partner.steamgames.com/doc/features/leaderboards");
        }
    }
}
#endif