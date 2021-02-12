#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.Scriptable;
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HeathenEngineering.SteamApi.Networking
{
    /// <summary>
    /// Handles configuration and tracking for Steam Lobby
    /// </summary>
    /// <remarks>
    /// <para>
    /// Steam allows users to be in 1 'regular' lobby and up to 2 'invisible' lobbies.
    /// This system simply manages a list of lobbies and doesn't try to enforce Valve's rule for 1 regular and 2 invisible.
    /// It is up to the indavidual game developer to handle lobby types and enforce rules regarding how many lobbies a user can be in.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = "Steamworks/Networking/Lobby Settings")]
    [Serializable]
    public class SteamworksLobbySettings : ScriptableObject
    {
        public readonly List<SteamLobby> lobbies = new List<SteamLobby>();

        /// <summary>
        /// Returns the <see cref="SteamLobby"/> object that matches the provided id
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby to find</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This queries the <see cref="lobbies"/> member. It does not search against Valve's backend e.g. it is not a Lobby Search/browse
        /// </para>
        /// </remarks>
        public SteamLobby this[CSteamID lobbyId]
        {
            get => lobbies.FirstOrDefault(p => p.id == lobbyId);
        }

        /// <summary>
        /// Controls the further Steam distance that will be searched for a lobby
        /// </summary>
        /// <remarks>
        /// <see cref="maxDistanceFilter"/> is used during <see cref="QuickMatch(LobbyHunterFilter, string, bool)"/> operations to determin the maximum distance the quick mach should search when expanding.
        /// </remarks>
        [Header("Quick Match Settings")]
        [FormerlySerializedAs("MaxDistanceFilter")]
        public ELobbyDistanceFilter maxDistanceFilter = ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault;
        
        /// <summary>
        /// Is the user in a lobby
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will test all lobbies listed in the <see cref="lobbies"/> field to determin if the user is a member.
        /// If the user is a member of any of the lobbies the result will be true.
        /// </para>
        /// </remarks>
        public bool InLobby
        {
            get
            {
                if (lobbies.Any(p => p != null && p.User != null))
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// True if the system is tracking a lobby
        /// </summary>
        public bool HasLobby
        {
            get
            {
                return lobbies.Any(p => p.id != CSteamID.Nil);
            }
        }
        
        /// <summary>
        /// True while the system is waiting for search result responce
        /// </summary>
        /// <remarks>
        /// </remarks>
        public bool IsSearching
        {
            get { return standardSearch; }
        }

        /// <summary>
        /// Returns true while the system is performing a quick search
        /// </summary>
        public bool IsQuickSearching
        {
            get { return quickMatchSearch; }
        }
        
        [HideInInspector]
        public ISteamworksLobbyManager Manager;

        #region Internal Data
        [NonSerialized]
        private bool standardSearch = false;
        [NonSerialized]
        private bool quickMatchSearch = false;
        [NonSerialized]
        private bool callbacksRegistered = false;
        private LobbyHunterFilter createLobbyFilter;
        private LobbyHunterFilter quickMatchFilter;
        #endregion

        #region Callbacks
        private CallResult<LobbyCreated_t> m_LobbyCreated;
        private Callback<LobbyEnter_t> m_LobbyEntered;
        private Callback<GameLobbyJoinRequested_t> m_GameLobbyJoinRequested;
        private Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;
        private CallResult<LobbyMatchList_t> m_LobbyMatchList;
        private Callback<LobbyGameCreated_t> m_LobbyGameCreated;
        private Callback<LobbyDataUpdate_t> m_LobbyDataUpdated;
        private Callback<LobbyChatMsg_t> m_LobbyChatMsg;
        #endregion

        #region Events
        
        /// <summary>
        /// Occures when a request to join the lobby has been recieved such as through Steam's invite friend dialog in the Steam Overlay
        /// </summary>
        [HideInInspector]
        public UnityGameLobbyJoinRequestedEvent OnGameLobbyJoinRequest = new UnityGameLobbyJoinRequestedEvent();
        /// <summary>
        /// Occures when list of Lobbies is retured from a search
        /// </summary>
        [HideInInspector]
        public UnityLobbyHunterListEvent OnLobbyMatchList = new UnityLobbyHunterListEvent();
        /// <summary>
        /// Occures when a lobby is created by the player
        /// </summary>
        /// <remarks>
        /// <para>
        /// The data from this event can be used to fetch the newly created lobby. A demonstration of this is availabel in the example below.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public SteamworksLobbySettings lobbySettings;
        /// ...
        /// void Start()
        /// {
        ///    lobbySettings.OnLobbyCreate.AddListener(HandleOnLobbyCreated);
        /// }
        /// ...
        /// private void HandleOnLobbyCreated(LobbyCreated_t param)
        /// {
        ///    var myNewLobby = lobbySettings[param.m_ulSteamIDLobby];
        /// }
        /// </code>
        /// </example>
        [HideInInspector]
        public UnityLobbyCreatedEvent OnLobbyCreated = new UnityLobbyCreatedEvent();


        
        /// <summary>
        /// Occures when the player joins a lobby
        /// </summary>
        [HideInInspector]
        public UnityLobbyEvent OnLobbyEnter = new UnityLobbyEvent();
        /// <summary>
        /// Occures when the player leaves a lobby
        /// </summary>
        [HideInInspector]
        public UnityLobbyEvent OnLobbyExit = new UnityLobbyEvent();

        /// <summary>
        /// Occures when the host of the lobby starts the game e.g. sets game server data on the lobby
        /// </summary>
        [HideInInspector]
        public UnityLobbyGameCreatedEvent OnGameServerSet = new UnityLobbyGameCreatedEvent();
        /// <summary>
        /// Occures when lobby chat metadata has been updated such as a kick or ban.
        /// </summary>
        [HideInInspector]
        public UnityLobbyChatUpdateEvent OnLobbyChatUpdate = new UnityLobbyChatUpdateEvent();
        /// <summary>
        /// Occures when a quick match search fails to return a lobby match
        /// </summary>
        [HideInInspector]
        public UnityEvent QuickMatchFailed = new UnityEvent();
        /// <summary>
        /// Occures when a search for a lobby has started
        /// </summary>
        [HideInInspector]
        public UnityEvent SearchStarted = new UnityEvent();
        /// <summary>
        /// Occures when a lobby chat message is recieved
        /// </summary>
        [HideInInspector]
        public LobbyChatMessageEvent OnChatMessageReceived = new LobbyChatMessageEvent();
        /// <summary>
        /// Occures when a member of the lobby chat enters the chat
        /// </summary>
        [HideInInspector]
        public SteamworksLobbyMemberEvent ChatMemberStateChangeEntered = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when a member of the lobby chat leaves the chat
        /// </summary>
        [HideInInspector]
        public UnityPersonaEvent ChatMemberStateChangeLeft = new UnityPersonaEvent();
        /// <summary>
        /// Occures when a member of the lobby chat is disconnected from the chat
        /// </summary>
        [HideInInspector]
        public UnityPersonaEvent ChatMemberStateChangeDisconnected = new UnityPersonaEvent();
        /// <summary>
        /// Occures when a member of the lobby chat is kicked out of the chat
        /// </summary>
        [HideInInspector]
        public UnityPersonaEvent ChatMemberStateChangeKicked = new UnityPersonaEvent();
        /// <summary>
        /// Occures when a member of the lobby chat is banned from the chat
        /// </summary>
        [HideInInspector]
        public UnityPersonaEvent ChatMemberStateChangeBanned = new UnityPersonaEvent();
        #endregion

        /// <summary>
        /// Typically called by the HeathenSteamManager.OnEnable()
        /// This registeres the Valve callbacks and CallResult deligates
        /// </summary>
        public void Initalize()
        {
            if (SteamSettings.current.Initialized)
            {   
                if (!callbacksRegistered)
                {
                    callbacksRegistered = true;
                    m_LobbyCreated = CallResult<LobbyCreated_t>.Create(HandleLobbyCreated);
                    m_LobbyEntered = Callback<LobbyEnter_t>.Create(HandleLobbyEntered);
                    m_GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(HandleGameLobbyJoinRequested);
                    m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(HandleLobbyChatUpdate);
                    m_LobbyMatchList = CallResult<LobbyMatchList_t>.Create(HandleLobbyMatchList);
                    m_LobbyGameCreated = Callback<LobbyGameCreated_t>.Create(HandleLobbyGameCreated);
                    m_LobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(HandleLobbyDataUpdate);
                    m_LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(HandleLobbyChatMessage);
                }
            }
        }

        #region Callbacks
        private void HandleLobbyList(SteamLobbyLobbyList lobbyList)
        {
            int lobbyCount = lobbyList.Count;

            if (quickMatchSearch)
            {
                if (lobbyCount == 0)
                {
                    if (!quickMatchFilter.useDistanceFilter)
                        quickMatchFilter.useDistanceFilter = true;

                    switch (quickMatchFilter.distanceOption)
                    {
                        case ELobbyDistanceFilter.k_ELobbyDistanceFilterClose:
                            if ((int)maxDistanceFilter >= 1)
                            {
                                quickMatchFilter.distanceOption = ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault;
                                FindQuickMatch();
                            }
                            else
                            {
                                HandleQuickMatchFailed();
                                return;
                            }
                            break;
                        case ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault:
                            if ((int)maxDistanceFilter >= 2)
                            {
                                quickMatchFilter.distanceOption = ELobbyDistanceFilter.k_ELobbyDistanceFilterFar;
                                FindQuickMatch();
                            }
                            else
                            {
                                HandleQuickMatchFailed();
                                return;
                            }
                            break;
                        case ELobbyDistanceFilter.k_ELobbyDistanceFilterFar:
                            if ((int)maxDistanceFilter >= 3)
                            {
                                quickMatchFilter.distanceOption = ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide;
                                FindQuickMatch();
                            }
                            else
                            {
                                HandleQuickMatchFailed();
                                return;
                            }
                            break;
                        case ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide:
                            HandleQuickMatchFailed();
                            return;
                    }
                }
                else
                {
                    //We got a hit, the top option should be the best option so join it
                    var lobby = SteamMatchmaking.GetLobbyByIndex(0);
                    JoinLobby(lobby);
                }
            }
        }

        private void HandleQuickMatchFailed()
        {
            quickMatchSearch = false;
            QuickMatchFailed.Invoke();
        }

        private void FindQuickMatch()
        {
            if (!callbacksRegistered)
                Initalize();

            SetLobbyFilter(quickMatchFilter);

            var call = SteamMatchmaking.RequestLobbyList();
            m_LobbyMatchList.Set(call, HandleLobbyMatchList);

            SearchStarted.Invoke();
        }

        private void SetLobbyFilter(LobbyHunterFilter LobbyFilter)
        {
            if (LobbyFilter.useSlotsAvailable)
                SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(LobbyFilter.requiredOpenSlots);

            if (LobbyFilter.useDistanceFilter)
                SteamMatchmaking.AddRequestLobbyListDistanceFilter(LobbyFilter.distanceOption);

            if (LobbyFilter.maxResults > 0)
                SteamMatchmaking.AddRequestLobbyListResultCountFilter(LobbyFilter.maxResults);

            if (LobbyFilter.numberValues != null)
            {
                foreach (var f in LobbyFilter.numberValues)
                    SteamMatchmaking.AddRequestLobbyListNumericalFilter(f.key, f.value, f.method);
            }

            if (LobbyFilter.nearValues != null)
            {
                foreach (var f in LobbyFilter.nearValues)
                    SteamMatchmaking.AddRequestLobbyListNearValueFilter(f.key, f.value);
            }

            if (LobbyFilter.stringValues != null)
            {
                foreach (var f in LobbyFilter.stringValues)
                    SteamMatchmaking.AddRequestLobbyListStringFilter(f.key, f.value, f.method);
            }
        }
        #endregion  

        #region Callback Handlers
        void HandleLobbyGameCreated(LobbyGameCreated_t param)
        {
            var lobby = lobbies.FirstOrDefault(p => p.id.m_SteamID == param.m_ulSteamIDLobby);

            if (lobby != null)
            {
                lobby.HandleLobbyGameCreated(param);
                OnGameServerSet.Invoke(param);
            }
        }

        void HandleLobbyMatchList(LobbyMatchList_t pCallback, bool bIOFailure)
        {
            uint numLobbies = pCallback.m_nLobbiesMatching;
            var result = new SteamLobbyLobbyList();
            
            if (numLobbies <= 0)
            {
                if (quickMatchSearch)
                {
                    Debug.Log("Lobby match list returned (0), refining search paramiters.");
                    HandleLobbyList(result);
                }
                else
                {
                    Debug.Log("Lobby match list returned (" + numLobbies.ToString() + ")");
                    standardSearch = false;
                    OnLobbyMatchList.Invoke(result);
                }
            }
            else
            {
                Debug.Log("Lobby match list returned (" + numLobbies.ToString() + ")");
                for (int i = 0; i < numLobbies; i++)
                {
                    LobbyHunterLobbyRecord record = new LobbyHunterLobbyRecord();

                    record.metadata = new Dictionary<string, string>();
                    record.lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                    //record.hostId = SteamMatchmaking.GetLobbyOwner(record.lobbyId);
                    //int memberCount = SteamMatchmaking.GetNumLobbyMembers(record.lobbyId);
                    record.maxSlots = SteamMatchmaking.GetLobbyMemberLimit(record.lobbyId);
                    

                    int dataCount = SteamMatchmaking.GetLobbyDataCount(record.lobbyId);

                    var matchLobby = lobbies.FirstOrDefault(p => p.id == record.lobbyId);

                    if (matchLobby != null)
                    {
                        Debug.Log("Browsed our own lobby and found " + dataCount.ToString() + " metadata records.");
                    }

                    for (int ii = 0; ii < dataCount; ii++)
                    {
                        bool isUs = matchLobby != null;
                        string key;
                        string value;
                        if (SteamMatchmaking.GetLobbyDataByIndex(record.lobbyId, ii, out key, Constants.k_nMaxLobbyKeyLength, out value, Constants.k_cubChatMetadataMax))
                        {
                            record.metadata.Add(key, value);
                            if (key == "name")
                                record.name = value;
                            if (key == "OwnerID")
                            {
                                ulong val;
                                if (ulong.TryParse(value, out val))
                                {
                                    record.hostId = new CSteamID(val);
                                }
                            }
                        }
                    }

                    result.Add(record);
                }

                if (quickMatchSearch)
                {
                    HandleLobbyList(result);
                }
                else
                {
                    standardSearch = false;
                    OnLobbyMatchList.Invoke(result);
                }
            }
        }

        void HandleLobbyChatUpdate(LobbyChatUpdate_t param)
        {
            var lobby = lobbies.FirstOrDefault(p => p.id.m_SteamID == param.m_ulSteamIDLobby);
            lobby.HandleLobbyChatUpdate(param);

            OnLobbyChatUpdate.Invoke(param);
        }

        void HandleGameLobbyJoinRequested(GameLobbyJoinRequested_t param)
        {
            //JoinLobby(param.m_steamIDLobby);
            OnGameLobbyJoinRequest.Invoke(param);
        }

        void HandleLobbyEntered(LobbyEnter_t param)
        {
            var lobby = lobbies.FirstOrDefault(p => p.id.m_SteamID == param.m_ulSteamIDLobby);
            if(lobby == null)
            {
                lobby = new SteamLobby(new CSteamID(param.m_ulSteamIDLobby));
                lobby.OnExitLobby.AddListener(HandleExitLobby);
                lobbies.Add(lobby);
            }

            OnLobbyEnter.Invoke(lobby);
        }

        void HandleLobbyCreated(LobbyCreated_t param, bool bIOFailure)
        {
            var lobby = lobbies.FirstOrDefault(p => p.id.m_SteamID == param.m_ulSteamIDLobby);
            if (lobby == null)
            {
                lobby = new SteamLobby(new CSteamID(param.m_ulSteamIDLobby));
                lobby.OnExitLobby.AddListener(HandleExitLobby);
                lobbies.Add(lobby);
            }

            OnLobbyCreated.Invoke(param);
        }

        void HandleLobbyDataUpdate(LobbyDataUpdate_t param)
        {
            var lobby = lobbies.FirstOrDefault(p => p.id.m_SteamID == param.m_ulSteamIDLobby);
            lobby.HandleLobbyDataUpdate(param);
        }

        void HandleLobbyChatMessage(LobbyChatMsg_t param)
        {
            var lobby = lobbies.FirstOrDefault(p => p.id.m_SteamID == param.m_ulSteamIDLobby);
            var message = lobby.HandleLobbyChatMessage(param);

            if (message != null)
                OnChatMessageReceived.Invoke(message);
        }

        void HandleExitLobby(SteamLobby lobby)
        {
            lobbies.RemoveAll(p => p.id == lobby.id);
            OnLobbyExit.Invoke(lobby);
        }
        #endregion

        #region Deprecated Members

        /// <summary>
        /// Depricated event.
        /// </summary>
        /// <remarks>
        /// OnLobbyDataChanged member is no longer used at the settings level, please use SteamLobby.OnLobbyDataChanged e.g. lobbySettings.lobbies[0].OnLobbyDataChanged
        /// </remarks>
        [Obsolete("OnLobbyDataChanged member is no longer used at the settings level, please use SteamLobby.OnLobbyDataChanged e.g. lobbySettings.lobbies[0].OnLobbyDataChanged", true)]
        [HideInInspector]
        public UnityEvent OnLobbyDataChanged { get; }

        /// <summary>
        /// Depricated event.
        /// </summary>
        /// <remarks>
        /// OnOwnershipChange member is no longer used at the settings level, please use SteamLobby.OnOwnershipChange e.g. lobbySettings.lobbies[0].OnOwnershipChange
        /// </remarks>
        [Obsolete("OnOwnershipChange member is no longer used at the settings level, please use SteamLobby.OnOwnershipChange e.g. lobbySettings.lobbies[0].OnOwnershipChange", true)]
        [HideInInspector]
        public SteamworksLobbyMemberEvent OnOwnershipChange { get; }

        /// <summary>
        /// Depricated event.
        /// </summary>
        /// <remarks>
        /// OnMemberJoined member is no longer used at the settings level, please use SteamLobby.OnMemberJoined e.g. lobbySettings.lobbies[0].OnMemberJoined
        /// </remarks>
        [Obsolete("OnMemberJoined member is no longer used at the settings level, please use SteamLobby.OnMemberJoined e.g. lobbySettings.lobbies[0].OnMemberJoined", true)]
        [HideInInspector]
        public SteamworksLobbyMemberEvent OnMemberJoined { get; }

        /// <summary>
        /// Depricated event.
        /// </summary>
        /// <remarks>
        /// OnMemberLeft member is no longer used at the settings level, please use SteamLobby.OnMemberLeft e.g. lobbySettings.lobbies[0].OnMemberLeft
        /// </remarks>
        [Obsolete("OnMemberLeft member is no longer used at the settings level, please use SteamLobby.OnMemberLeft e.g. lobbySettings.lobbies[0].OnMemberLeft", true)]
        [HideInInspector]
        public SteamworksLobbyMemberEvent OnMemberLeft { get; }

        /// <summary>
        /// Depricated event.
        /// </summary>
        /// <remarks>
        /// OnMemberDataChanged member is no longer used at the settings level, please use SteamLobby.OnMemberDataChanged e.g. lobbySettings.lobbies[0].OnMemberDataChanged
        /// </remarks>
        [Obsolete("OnMemberDataChanged member is no longer used at the settings level, please use SteamLobby.OnMemberDataChanged e.g. lobbySettings.lobbies[0].OnMemberDataChanged", true)]
        [HideInInspector]
        public SteamworksLobbyMemberEvent OnMemberDataChanged { get; }

        /// <summary>
        /// Depricated event.
        /// </summary>
        /// <remarks>
        /// LobbyDataUpdateFailed member is no longer used at the settings level, please use SteamLobby.LobbyDataUpdateFailed e.g. lobbySettings.lobbies[0].LobbyDataUpdateFailed
        /// </remarks>
        [Obsolete("LobbyDataUpdateFailed member is no longer used at the settings level, please use SteamLobby.LobbyDataUpdateFailed e.g. lobbySettings.lobbies[0].LobbyDataUpdateFailed", true)]
        [HideInInspector]
        public UnityEvent LobbyDataUpdateFailed { get; }

        /// <summary>
        /// Depricated event.
        /// </summary>
        /// <remarks>
        /// OnKickedFromLobby member is no longer used at the settings level, please use SteamLobby.OnKickedFromLobby e.g. lobbySettings.lobbies[0].OnKickedFromLobby
        /// </remarks>
        [Obsolete("OnKickedFromLobby member is no longer used at the settings level, please use SteamLobby.OnKickedFromLobby e.g. lobbySettings.lobbies[0].OnKickedFromLobby", true)]
        [HideInInspector]
        public UnityEvent OnKickedFromLobby { get; }

        /// <summary>
        /// Depricated is host member.
        /// This will always be false and should throw an exception if called in code.
        /// </summary>
        /// <remarks>
        /// You can check if the local user is the host of any specific lobby by calling that lobby's IsHost member e.g. lobbySettings.lobbies[0].IsHost
        /// </remarks>
        [Obsolete("IsHost member is no longer used, please use SteamLobby.IsHost e.g. lobbySettings.lobbies[0].IsHost", true)]
        public bool IsHost { get; }

        /// <summary>
        /// Depricated has game server member.
        /// This will always be false and should throw an exception if called in code.
        /// </summary>
        /// <remarks>
        /// You can check if the has game server on any specific lobby by calling that lobby's HasGameServer member e.g. lobbySettings.lobbies[0].HasGameServer
        /// </remarks>
        [Obsolete("HasGameServer member is no longer used, please use SteamLobby.HasGameServer e.g. lobbySettings.lobbies[0].HasGameServer", true)]
        public bool HasGameServer { get; }

        /// <summary>
        /// Depricated Game Server Information member.
        /// This will always be false and should throw an exception if called in code.
        /// </summary>
        /// <remarks>
        /// You can check if the has game server on any specific lobby by calling that lobby's GameServer member e.g. lobbySettings.lobbies[0].GameServer
        /// </remarks>
        [Obsolete("GameServerInformation member is no longer used, please use SteamLobby.GameServer e.g. lobbySettings.lobbies[0].GameServer", true)]
        public LobbyGameServerInformation GameServerInformation { get; }


        /// <summary>
        /// Depricated metadata member.
        /// This will be null and should throw an exception if called in code.
        /// </summary>
        /// <remarks>
        /// You can use the string Indexer on SteamLobby to access that Lobbies metadata e.g. 
        /// <code>myLobby["metadataKey"] = "This sets the value of metadataKey";</code>
        /// 
        /// </remarks>
        [Obsolete("Metadata member is no longer used on the settings object, please use SteamLobby[string metadataKey] to access a specific metadata field or use SteamLobby.GetMetadataEntries() to return an array of KeyValuePair<string key, string value> representing each field that can be iterated over such as in a foreach loop.", true)]
        public SteamworksLobbyMetadata Metadata { get; }

        /// <summary>
        /// Depricated create lobby command.
        /// This will throw an exception if called and its use will appear as an error in your compiler.
        /// </summary>
        /// <param name="lobbyFilter"></param>
        /// <param name="lobbyName"></param>
        /// <param name="lobbyType"></param>
        [Obsolete("CreateLobby(LobbyHunterFilter lobbyFilter, string lobbyName, ELobbyType lobbyType) is deprecated, please use CreateLobby(ELobbyType lobbyType, int memberCountLimit) instead.", true)]
        public void CreateLobby(LobbyHunterFilter lobbyFilter, string lobbyName, ELobbyType lobbyType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Depricated leve lobby command.
        /// This will throw an exception if called and its use will appear as an error in your compiler.
        /// </summary>
        [Obsolete("LeaveLobby is deprecated, please use the Leave method available on the SteamLobby object to leave a specific lobby, e.g. LobbySettings.lobbies[LobbyId].Leave();", true)]
        public void LeaveLobby()
        {
            throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// Creates a new empty (without metadata) lobby. The user will be added to the lobby on creation
        /// </summary>
        /// <param name="lobbyType">The type of lobby to be created ... see Valve's documentation regarding ELobbyType for more informaiton</param>
        /// <param name="memberCountLimit">The limit on the number of users that can join this lobby</param>
        public void CreateLobby(ELobbyType lobbyType, int memberCountLimit)
        {
            var call = SteamMatchmaking.CreateLobby(lobbyType, memberCountLimit);
            m_LobbyCreated.Set(call, HandleLobbyCreated);
        }

        /// <summary>
        /// Joins a steam lobby
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby to join</param>
        /// <remarks>
        /// See <see href="https://partner.steamgames.com/doc/api/ISteamMatchmaking#JoinLobby">JoinLobby</see> in Valve's documentation for more details.
        /// </remarks>
        public void JoinLobby(CSteamID lobbyId)
        {
            SteamMatchmaking.JoinLobby(lobbyId);
        }

        /// <summary>
        /// Joins a steam lobby
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby to join</param>
        /// <remarks>
        /// See <see href="https://partner.steamgames.com/doc/api/ISteamMatchmaking#JoinLobby">JoinLobby</see> in Valve's documentation for more details.
        /// </remarks>
        public void JoinLobby(ulong lobbyId)
        {
            SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));
        }

        /// <summary>
        /// Leaves the current lobby if any
        /// </summary>
        public void LeaveAllLobbies()
        {
            var tempList = lobbies.ToArray();

            foreach (var lobby in tempList)
                lobby.Leave();

            lobbies.Clear();
            tempList = null;
        }

        /// <summary>
        /// Searches for a matching lobby according to the provided filter data.
        /// Note that a search will only start if no search is currently running.
        /// </summary>
        /// <param name="LobbyFilter">Describes the metadata to search for in a lobby</param>
        public void FindMatch(LobbyHunterFilter LobbyFilter)
        {
            if (quickMatchSearch)
            {
                Debug.LogError("Attempted to search for a lobby while a quick search is processing. This search will be ignored, you must call CancelQuickMatch to abort a search before it completes, note that results may still come back resulting in the next match list not being as expected.");
                return;
            }

            standardSearch = true;

            SetLobbyFilter(LobbyFilter);

            var call = SteamMatchmaking.RequestLobbyList();
            m_LobbyMatchList.Set(call, HandleLobbyMatchList);

            SearchStarted.Invoke();
        }

        /// <summary>
        /// Starts a staged search for a matching lobby. Search will only start if no searches are currently running.
        /// </summary>
        /// <param name="LobbyFilter">The metadata of a lobby to search for</param>
        /// <param name="autoCreate">Should the system create a lobby if no matching lobby is found</param>
        /// <returns>True if the search was started, false otherwise.</returns>
        public bool QuickMatch(LobbyHunterFilter LobbyFilter)
        {
            if (!callbacksRegistered)
                Initalize();

            if (quickMatchSearch || standardSearch)
            {
                return false;
            }

            quickMatchSearch = true;
            quickMatchFilter = LobbyFilter;
            quickMatchFilter.distanceOption = ELobbyDistanceFilter.k_ELobbyDistanceFilterClose;
            quickMatchFilter.useDistanceFilter = true;
            FindQuickMatch();

            return true;
        }

        /// <summary>
        /// Terminates a quick search process
        /// Note that lobby searches are asynchronious and result may return after the cancelation
        /// </summary>
        public void CancelQuickMatch()
        {
            if (!callbacksRegistered)
                Initalize();

            if (quickMatchSearch)
            {
                quickMatchSearch = false;
                Debug.LogWarning("Quick Match search has been canceled, note that results may still come back resulting in the next match list not being as expected.");
            }
        }

        /// <summary>
        /// Terminates a standard search
        /// Note that lobby searches are asynchronious and result may return after the cancelation
        /// </summary>
        public void CancelStandardSearch()
        {
            if (!callbacksRegistered)
                Initalize();

            if (standardSearch)
            {
                standardSearch = false;
                Debug.LogWarning("Search has been canceled, note that results may still come back resulting in the next match list not being as expected.");
            }
        }

        /// <summary>
        /// Sends a chat message via Valve's Lobby Chat system to the first lobby in the <see cref="lobbies"/> list
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <remarks>
        /// <para>
        /// This method exists here for support of older single lobby systems. It is recomended that you use the SendChatMessage on the specific lobby you want to send a message on or that you use the overload that takes the lobby ID.
        /// </para>
        /// </remarks>
        public void SendChatMessage(string message)
        {
            if (lobbies.Count == 0)
                return;

            if (!callbacksRegistered)
                Initalize();

            byte[] MsgBody = System.Text.Encoding.UTF8.GetBytes(message);
            SteamMatchmaking.SendLobbyChatMsg(lobbies[0].id, MsgBody, MsgBody.Length);
        }

        /// <summary>
        /// Send a chat message to the indicated lobby
        /// </summary>
        /// <param name="lobbyId">The lobby to chat on</param>
        /// <param name="message">The message to be sent</param>
        public void SendChatMessage(CSteamID lobbyId, string message)
        {
            if (lobbies.Count == 0)
                return;

            if (!callbacksRegistered)
                Initalize();

            byte[] MsgBody = System.Text.Encoding.UTF8.GetBytes(message);
            SteamMatchmaking.SendLobbyChatMsg(lobbyId, MsgBody, MsgBody.Length);
        }

        /// <summary>
        /// Sets metadata on the first lobby, this can only be called by the host of the lobby
        /// </summary>
        /// <param name="key">The key of the metadata to set</param>
        /// <param name="value">The value of the metadata to set</param>
        /// <remarks>
        /// <para>
        /// This is here to support older single lobby code, it is recomended that you set data directly on the <see cref="SteamLobby"/> object or use the overload to specify the lobby you want to target.
        /// </para>
        /// </remarks>
        public void SetLobbyMetadata(string key, string value)
        {
            if (lobbies.Count == 0)
                return;

            if (!callbacksRegistered)
                Initalize();

            lobbies[0][key] = value;
        }

        /// <summary>
        /// Sets metadata on the first lobby, this can only be called by the host of the lobby
        /// </summary>
        /// <param name="key">The key of the metadata to set</param>
        /// <param name="value">The value of the metadata to set</param>
        public void SetLobbyMetadata(CSteamID lobbyId, string key, string value)
        {
            if (!callbacksRegistered)
                Initalize();

            this[lobbyId][key] = value;
        }

        /// <summary>
        /// Sets metadata for the player on the first lobby
        /// </summary>
        /// <param name="key">The key of the metadata to set</param>
        /// <param name="value">The value of the metadata to set</param>
        public void SetMemberMetadata(string key, string value)
        {
            if (lobbies.Count == 0)
                return;

            if (!callbacksRegistered)
                Initalize();

            lobbies[0].User[key] = value;
        }

        /// <summary>
        /// Sets metadata for the player on the first lobby
        /// </summary>
        /// <param name="key">The key of the metadata to set</param>
        /// <param name="value">The value of the metadata to set</param>
        public void SetMemberMetadata(CSteamID lobbyId, string key, string value)
        {
            if (!callbacksRegistered)
                Initalize();

            this[lobbyId].User[key] = value;
        }

        /// <summary>
        /// Sets the lobby game server e.g. game start using the lobby Host as the server ID
        /// </summary>
        /// <remarks>
        /// <para>
        /// This assumes you want to set the game server on the first lobby. It exists to support older code that used a single lobby system.
        /// It is recomended that you call <see cref="SteamLobby.SetGameServer"/> directly on the lobby you want or use the overload to indicate the lobby.
        /// </para>
        /// <para>
        /// This will trigger GameServerSet on all members of the lobby
        /// This should be called after the server is started
        /// </para>
        /// </remarks>
        public void SetLobbyGameServer()
        {
            if (!callbacksRegistered)
                Initalize();

            lobbies[0].SetGameServer();
        }

        /// <summary>
        /// Sets the lobby game server e.g. game start using the lobby Host as the server ID
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will trigger GameServerSet on all members of the lobby
        /// This should be called after the server is started
        /// </para>
        /// </remarks>
        public void SetLobbyGameServer(CSteamID lobbyId)
        {
            if (!callbacksRegistered)
                Initalize();

            this[lobbyId].SetGameServer();
        }

        /// <summary>
        /// Sets the lobby game server e.g. game start
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="serverId"></param>
        /// <remarks>
        /// <para>
        /// This assumes you want to set the game server on the first lobby. It exists to support older code that used a single lobby system.
        /// It is recomended that you call <see cref="SteamLobby.SetGameServer"/> directly on the lobby you want or use the overload to indicate the lobby.
        /// </para>
        /// <para>
        /// This will trigger GameServerSet on all members of the lobby
        /// This should be called after the server is started
        /// </para>
        /// </remarks>
        public void SetLobbyGameServer(string ipAddress, ushort port, CSteamID serverId)
        {
            lobbies[0].SetGameServer(ipAddress, port, serverId);
        }

        /// <summary>
        /// Sets the lobby game server e.g. game start
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="serverId"></param>
        /// <remarks>
        /// <para>
        /// This will trigger GameServerSet on all members of the lobby
        /// This should be called after the server is started
        /// </para>
        /// </remarks>
        public void SetLobbyGameServer(CSteamID lobbyId, string ipAddress, ushort port, CSteamID serverId)
        {
            this[lobbyId].SetGameServer(ipAddress, port, serverId);
        }

        /// <summary>
        /// Sets the lobby as joinable or not. The default is that a lobby is joinable.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// <para>
        /// This assumes you want to set the game server on the first lobby. It exists to support older code that used a single lobby system.
        /// It is recomended that you call <see cref="SteamLobby.Joinable"/> directly on the lobby you want or use the overload to indicate the lobby.
        /// </para>
        /// </remarks>
        public void SetLobbyJoinable(bool value)
        {
            lobbies[0].Joinable = value;
        }

        /// <summary>
        /// Sets the lobby as joinable or not. The default is that a lobby is joinable.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// <para>
        /// This assumes you want to set the game server on the first lobby. It exists to support older code that used a single lobby system.
        /// It is recomended that you call <see cref="SteamLobby.Joinable"/> directly on the lobby you want or use the overload to indicate the lobby.
        /// </para>
        /// </remarks>
        public void SetLobbyJoinable(CSteamID lobbyId, bool value)
        {
            this[lobbyId].Joinable = value;
        }

        /// <summary>
        /// Returns information about the lobbies game server
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This assumes you want to set the game server on the first lobby. It exists to support older code that used a single lobby system.
        /// It is recomended that you call <see cref="SteamLobby.GameServer"/> directly on the lobby you want or use the overload to indicate the lobby.
        /// </para>
        /// </remarks>
        public LobbyGameServerInformation GetGameServer()
        {
            return lobbies[0].GameServer;
        }

        /// <summary>
        /// Returns information about the lobbies game server
        /// </summary>
        /// <param name="lobbyId"></param>
        /// <returns></returns>
        public LobbyGameServerInformation GetGameServer(CSteamID lobbyId)
        {
            return this[lobbyId].GameServer;
        }

        /// <summary>
        /// Marks the user to be removed
        /// </summary>
        /// <param name="memberId"></param>
        /// <remarks>
        /// <para>
        /// This assumes you want to set the game server on the first lobby. It exists to support older code that used a single lobby system.
        /// It is recomended that you call <see cref="SteamLobby.GameServer"/> directly on the lobby you want or use the overload to indicate the lobby.
        /// </para>
        /// This creates an entry in the metadata named z_heathenKick which contains a string array of Ids of users that should leave the lobby.
        /// When users detect their ID in the string they will automatically leave the lobby on leaving the lobby the users ID will be removed from the array.
        /// </remarks>
        public void KickMember(CSteamID memberId)
        {
            lobbies[0].KickMember(memberId);
        }

        /// <summary>
        /// Marks the user to be removed
        /// </summary>
        /// <param name="memberId"></param>
        /// <remarks>
        /// This creates an entry in the metadata named z_heathenKick which contains a string array of Ids of users that should leave the lobby.
        /// When users detect their ID in the string they will automatically leave the lobby on leaving the lobby the users ID will be removed from the array.
        /// </remarks>
        public void KickMember(CSteamID lobbyId, CSteamID memberId)
        {
            this[lobbyId].KickMember(memberId);
        }

        /// <summary>
        /// Sets the indicated user as the new owner of the lobby
        /// </summary>
        /// <param name="newOwner"></param>
        /// <remarks>
        /// <para>
        /// This assumes you want to set the game server on the first lobby. It exists to support older code that used a single lobby system.
        /// It is recomended that you call <see cref="SteamLobby.OwnerId"/> directly on the lobby you want or use the overload to indicate the lobby.
        /// </para>
        /// <para>
        /// This does not effect the NetworkManager or other networking funcitonality it only changes the ownership of a lobby
        /// </para>
        /// </remarks>
        public void ChangeOwner(CSteamID newOwner)
        {
            lobbies[0].OwnerId = newOwner;
        }

        /// <summary>
        /// Sets the indicated user as the new owner of the lobby
        /// </summary>
        /// <param name="newOwner"></param>
        /// <remarks>
        /// <para>
        /// This does not effect the NetworkManager or other networking funcitonality it only changes the ownership of a lobby
        /// </para>
        /// </remarks>
        public void ChangeOwner(CSteamID lobbyId, CSteamID newOwner)
        {
            this[lobbyId].OwnerId = newOwner;
        }
    }

}
#endif