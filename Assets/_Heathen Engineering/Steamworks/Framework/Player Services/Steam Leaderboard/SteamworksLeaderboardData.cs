#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// <para>Represents a Steam Leaderboard and manages its entries and quries</para>
    /// <para>To create a new <see cref="SteamworksLeaderboardData"/> object in your project right click in a folder in your project and select</para>
    /// <para>Create >> Steamworks >> Player Services >> Leaderboard Data</para>
    /// </summary>
    [CreateAssetMenu(menuName = "Steamworks/Player Services/Leaderboard Data")]
    public class SteamworksLeaderboardData : ScriptableObject
    {
        /// <summary>
        /// Should the board be created if missing on the target app
        /// </summary>
        public bool createIfMissing;
        /// <summary>
        /// If creating a board what sort method should be applied
        /// </summary>
        public ELeaderboardSortMethod sortMethod;
        /// <summary>
        /// If creating a board what display type is it
        /// </summary>
        public ELeaderboardDisplayType displayType;
        /// <summary>
        /// What is the name of the board ... if this is not to be created at run time then this must match the name as it appears in Steam
        /// </summary>
        public string leaderboardName;
        /// <summary>
        /// How many detail entries should be allowed on entries from this board
        /// </summary>
        public int MaxDetailEntries = 0;
        /// <summary>
        /// What is the leaderboard ID ... this is nullable if null then no leaderboard has been connected
        /// </summary>
        [HideInInspector]
        public SteamLeaderboard_t? LeaderboardId;
        /// <summary>
        /// What is the current player's entry for this board ... this is nullable if null then no etnry was found
        /// </summary>
        [HideInInspector]
        public LeaderboardEntry_t? UserEntry = null;
        /// <summary>
        /// Occures when the leaderboard is found
        /// </summary>
        public UnityEvent BoardFound = new UnityEvent();
        /// <summary>
        /// Occures when query results return from a query submited to the Steam backend
        /// </summary>
        public LeaderboardScoresDownloadedEvent OnQueryResults = new LeaderboardScoresDownloadedEvent();
        /// <summary>
        /// Occures when the players rank is loaded from Steam
        /// </summary>
        public UnityLeaderboardRankUpdateEvent UserRankLoaded = new UnityLeaderboardRankUpdateEvent();
        /// <summary>
        /// Occures when the players rank changes
        /// </summary>
        public UnityLeaderboardRankChangeEvent UserRankChanged = new UnityLeaderboardRankChangeEvent();
        /// <summary>
        /// Occures when the player achieves a new high rank in this board
        /// </summary>
        public UnityLeaderboardRankChangeEvent UserNewHighRank = new UnityLeaderboardRankChangeEvent();

        private CallResult<LeaderboardFindResult_t> OnLeaderboardFindResultCallResult;
        private CallResult<LeaderboardScoresDownloaded_t> OnLeaderboardScoresDownloadedCallResult;
        private CallResult<LeaderboardScoreUploaded_t> OnLeaderboardScoreUploadedCallResult;

        /// <summary>
        /// Registers the board on Steam creating if configured to do so or locating if not.
        /// </summary>
        public void Register()
        {
            OnLeaderboardFindResultCallResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult);
            OnLeaderboardScoresDownloadedCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
            OnLeaderboardScoreUploadedCallResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);

            if (createIfMissing)
                FindOrCreateLeaderboard(sortMethod, displayType);
            else
                FindLeaderboard();
        }

        private void FindOrCreateLeaderboard(ELeaderboardSortMethod sortMethod, ELeaderboardDisplayType displayType)
        {
            var handle = SteamUserStats.FindOrCreateLeaderboard(leaderboardName, sortMethod, displayType);
            OnLeaderboardFindResultCallResult.Set(handle);
        }

        private void FindLeaderboard()
        {
            var handle = SteamUserStats.FindLeaderboard(leaderboardName);
            OnLeaderboardFindResultCallResult.Set(handle);
        }

        /// <summary>
        /// Refreshes the user's entry for this board
        /// </summary>
        public void RefreshUserEntry()
        {
            if (!LeaderboardId.HasValue)
            {
                Debug.LogError(name + " Leaderboard Data Object, cannot download scores, the leaderboard has not been initalized and cannot download scores.");
                return;
            }

            CSteamID[] users = new CSteamID[] { SteamUser.GetSteamID() };
            var handle = SteamUserStats.DownloadLeaderboardEntriesForUsers(LeaderboardId.Value, users, 1);
            OnLeaderboardScoresDownloadedCallResult.Set(handle, OnLeaderboardUserRefreshRequest);
        }

        /// <summary>
        /// Uploads a score for the player to this board
        /// </summary>
        /// <param name="score"></param>
        /// <param name="method"></param>
        public void UploadScore(int score, ELeaderboardUploadScoreMethod method)
        {
            if (!LeaderboardId.HasValue)
            {
                Debug.LogError(name + " Leaderboard Data Object, cannot upload scores, the leaderboard has not been initalized and cannot upload scores.");
                return;
            }

            var handle = SteamUserStats.UploadLeaderboardScore(LeaderboardId.Value, method, score, null, 0);
            OnLeaderboardScoreUploadedCallResult.Set(handle);
        }

        /// <summary>
        /// Uploads a score for the player to this board
        /// </summary>
        /// <param name="score"></param>
        /// <param name="method"></param>
        public void UploadScore(int score, int[] scoreDetails, ELeaderboardUploadScoreMethod method)
        {
            if (!LeaderboardId.HasValue)
            {
                Debug.LogError(name + " Leaderboard Data Object, cannot upload scores, the leaderboard has not been initalized and cannot upload scores.");
                return;
            }

            var handle = SteamUserStats.UploadLeaderboardScore(LeaderboardId.Value, method, score, scoreDetails, scoreDetails.Length);
            OnLeaderboardScoreUploadedCallResult.Set(handle);
        }

        /// <summary>
        /// Get the top entries for this board
        /// </summary>
        /// <param name="count"></param>
        public void QueryTopEntries(int count)
        {
            QueryEntries(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, count);
        }

        /// <summary>
        /// Get the entries for the player's friends from this board
        /// </summary>
        /// <param name="aroundPlayer"></param>
        public void QueryFriendEntries(int aroundPlayer)
        {
            QueryEntries(ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends, -aroundPlayer, aroundPlayer);
        }

        /// <summary>
        /// Get entries for records near the player's record in this board
        /// </summary>
        /// <param name="aroundPlayer"></param>
        public void QueryPeerEntries(int aroundPlayer)
        {
            QueryEntries(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser, -aroundPlayer, aroundPlayer);
        }

        /// <summary>
        /// Query for entries from this baord
        /// </summary>
        /// <param name="requestType"></param>
        /// <param name="rangeStart"></param>
        /// <param name="rangeEnd"></param>
        public void QueryEntries(ELeaderboardDataRequest requestType, int rangeStart, int rangeEnd)
        {
            if (!LeaderboardId.HasValue)
            {
                Debug.LogError(name + " Leaderboard Data Object, cannot download scores, the leaderboard has not been initalized and cannot download scores.");
                return;
            }

            var handle = SteamUserStats.DownloadLeaderboardEntries(LeaderboardId.Value, requestType, rangeStart, rangeEnd);
            OnLeaderboardScoresDownloadedCallResult.Set(handle, OnLeaderboardScoresDownloaded);
        }

        private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t param, bool bIOFailure)
        {
            if (param.m_bSuccess == 0 || bIOFailure)
                Debug.LogError(name + " Leaderboard Data Object, failed to upload score to Steam: Success code = " + param.m_bSuccess, this);

            RefreshUserEntry();
        }

        private void OnLeaderboardUserRefreshRequest(LeaderboardScoresDownloaded_t param, bool bIOFailure)
        {
            ProcessScoresDownloaded(param, bIOFailure);
        }

        private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t param, bool bIOFailure)
        {
            var playerIncluded = ProcessScoresDownloaded(param, bIOFailure);

            //if (param.m_cEntryCount > 1 || (param.m_cEntryCount == 1 && !playerIncluded))
            OnQueryResults.Invoke(new LeaderboardScoresDownloaded() { bIOFailure = bIOFailure, scoreData = param, playerIncluded = playerIncluded });
        }

        private bool ProcessScoresDownloaded(LeaderboardScoresDownloaded_t param, bool bIOFailure)
        {
            bool playerIncluded = false;
            ///Check for the current users data in the record set and update accordingly
            if (!bIOFailure)
            {
                var userId = SteamUser.GetSteamID();

                for (int i = 0; i < param.m_cEntryCount; i++)
                {
                    LeaderboardEntry_t buffer;
                    int[] details = null;

                    if (MaxDetailEntries < 1)
                        SteamUserStats.GetDownloadedLeaderboardEntry(param.m_hSteamLeaderboardEntries, i, out buffer, details, MaxDetailEntries);
                    else
                    {
                        details = new int[MaxDetailEntries];
                        SteamUserStats.GetDownloadedLeaderboardEntry(param.m_hSteamLeaderboardEntries, i, out buffer, details, MaxDetailEntries);
                    }

                    if (buffer.m_steamIDUser.m_SteamID == userId.m_SteamID)
                    {
                        playerIncluded = true;
                        if (!UserEntry.HasValue || UserEntry.Value.m_nGlobalRank != buffer.m_nGlobalRank)
                        {
                            var l = new LeaderboardUserData()
                            {
                                leaderboardName = leaderboardName,
                                leaderboardId = LeaderboardId.Value,
                                entry = buffer,
                                details = details
                            };

                            var lc = new LeaderboardRankChangeData()
                            {
                                leaderboardName = leaderboardName,
                                leaderboardId = LeaderboardId.Value,
                                newEntry = buffer,
                                oldEntry = UserEntry.HasValue ? new LeaderboardEntry_t?(UserEntry.Value) : null
                            };

                            UserEntry = buffer;

                            UserRankLoaded.Invoke(l);
                            UserRankChanged.Invoke(lc);

                            if (lc.newEntry.m_nGlobalRank < (lc.oldEntry.HasValue ? lc.oldEntry.Value.m_nGlobalRank : int.MaxValue))
                            {
                                UserNewHighRank.Invoke(lc);
                            }
                        }
                        else
                        {
                            var l = new LeaderboardUserData()
                            {
                                leaderboardName = leaderboardName,
                                leaderboardId = LeaderboardId.Value,
                                entry = buffer,
                                details = details
                            };

                            UserEntry = buffer;
                            UserRankLoaded.Invoke(l);
                        }
                    }
                }
            }

            return playerIncluded;
        }

        private void OnLeaderboardFindResult(LeaderboardFindResult_t param, bool bIOFailure)
        {
            if (param.m_bLeaderboardFound == 0 || bIOFailure)
            {
                Debug.LogError("Failed to find leaderboard", this);
                return;
            }

            if (param.m_bLeaderboardFound != 0)
            {
                LeaderboardId = param.m_hSteamLeaderboard;
                BoardFound.Invoke();
                RefreshUserEntry();
            }
        }
    }
}
#endif