#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS 
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.Tools;
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Networking
{
    /// <summary>
    /// Handles registration of Steam callbacks for the Steam Matchmaking system aka Steam Lobby.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This component simply reflects the features and funcitons defined on the <see cref="SteamworksLobbySettings"/>, it does not define any data or provide any funcitonality not present on the <see cref="SteamworksLobbySettings"/> object.
    /// The intended use is that you will use this object to initalize the <see cref="SteamworksLobbySettings"/> object referenced at <see cref="LobbySettings"/>, this happens when this object is enabled. 
    /// You forgo using this componenet and instead use <see cref="SteamworksLobbySettings"/> in your own Lobby Manager. To do so simply add a reference to your lobby manager of:
    /// <code>public SteamworksLobbySettings lobbySettings;</code>
    /// Make sure you drag a <see cref="SteamworksLobbySettings"/> scriptable object to the reference on your lobby manager.
    /// Next add the <see cref="ISteamworksLobbyManager"/> interface as a reference to your lobby manager and implament its member funcitons ... in most cases you will default the members of <see cref="ISteamworksLobbyManager"/> to call the corasponding method on <see cref="SteamworksLobbySettings"/>.
    /// Implamenting the <see cref="ISteamworksLobbyManager"/> interface insures that other systems that use use lobby manager such as lobby browser can still interact with your custom lobby manager, <see cref="SteamLobbyDisplayList"/> and <see cref="UI.SteamworksLobbyChat"/> both depend on the <see cref="SteamworksLobbySettings.Manager"/> reference pointing to a valid <see cref="ISteamworksLobbyManager"/>.
    /// The <see cref="SteamLobbyDisplayList"/> and <see cref="UI.SteamworksLobbyChat"/> call through the <see cref="SteamworksLobbySettings"/> object to the <see cref="ISteamworksLobbyManager"/> to execute funciton so that any custom logic you want to run on these calls before or after call to the underlying lobby system can be implamented in your own custom lobby manager.
    /// </para>
    /// </remarks>
    public class SteamworksLobbyManager : HeathenBehaviour, ISteamworksLobbyManager
    {
        public SteamworksLobbySettings LobbySettings;

        #region Events
        /// <summary>
        /// Occures when a request to join the lobby has been recieved such as through Steam's invite friend dialog in the Steam Overlay
        /// </summary>
        public UnityGameLobbyJoinRequestedEvent OnGameLobbyJoinRequest = new UnityGameLobbyJoinRequestedEvent();
        /// <summary>
        /// Occures when list of Lobbies is retured from a search
        /// </summary>
        public UnityLobbyHunterListEvent OnLobbyMatchList = new UnityLobbyHunterListEvent();
        /// <summary>
        /// Occures when a lobby is created by the player
        /// </summary>
        public UnityLobbyCreatedEvent OnLobbyCreated = new UnityLobbyCreatedEvent();
        
        /// <summary>
        /// Occures when the player joins a lobby
        /// </summary>
        public UnityLobbyEvent OnLobbyEnter = new UnityLobbyEvent();
        public UnityLobbyEvent OnLobbyExit = new UnityLobbyEvent();
        /// <summary>
        /// Occures when the host of the lobby starts the game e.g. sets game server data on the lobby
        /// </summary>
        public UnityLobbyGameCreatedEvent OnGameServerSet = new UnityLobbyGameCreatedEvent();
        /// <summary>
        /// Occures when lobby chat metadata has been updated such as a kick or ban.
        /// </summary>
        public UnityLobbyChatUpdateEvent OnLobbyChatUpdate = new UnityLobbyChatUpdateEvent();
        /// <summary>
        /// Occures when a quick match search fails to return a lobby match
        /// </summary>
        public UnityEvent QuickMatchFailed = new UnityEvent();
        /// <summary>
        /// Occures when a search for a lobby has started
        /// </summary>
        public UnityEvent SearchStarted = new UnityEvent();
        /// <summary>
        /// Occures when a lobby chat message is recieved
        /// </summary>
        public LobbyChatMessageEvent OnChatMessageReceived = new LobbyChatMessageEvent();
        /// <summary>
        /// Occures when a member of the lobby chat enters the chat
        /// </summary>
        public SteamworksLobbyMemberEvent ChatMemberStateChangeEntered = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when a member of the lobby chat leaves the chat
        /// </summary>
        public UnityPersonaEvent ChatMemberStateChangeLeft = new UnityPersonaEvent();
        /// <summary>
        /// Occures when a member of the lobby chat is disconnected from the chat
        /// </summary>
        public UnityPersonaEvent ChatMemberStateChangeDisconnected = new UnityPersonaEvent();
        /// <summary>
        /// Occures when a member of the lobby chat is kicked out of the chat
        /// </summary>
        public UnityPersonaEvent ChatMemberStateChangeKicked = new UnityPersonaEvent();
        /// <summary>
        /// Occures when a member of the lobby chat is banned from the chat
        /// </summary>
        public UnityPersonaEvent ChatMemberStateChangeBanned = new UnityPersonaEvent();
        #endregion

        /// <summary>
        /// True while the system is waiting for search result responce
        /// </summary>
        /// <remarks>
        /// </remarks>
        public bool IsSearching
        {
            get { return LobbySettings.IsSearching; }
        }

        /// <summary>
        /// Returns true while the system is performing a quick search
        /// </summary>
        public bool IsQuickSearching
        {
            get { return LobbySettings.IsQuickSearching; }
        }

        #region Unity Methods
        private void OnEnable()
        {
            if (LobbySettings == null)
            {
                Debug.LogWarning("Lobby settings not found ... creating default settings");
                LobbySettings = ScriptableObject.CreateInstance<SteamworksLobbySettings>();
            }
            else if (LobbySettings.Manager != null && (object)LobbySettings.Manager != this)
            {
                Debug.LogWarning("Lobby settings already references a manager, this lobby manager will overwrite it. Please insure there is only 1 " + nameof(SteamworksLobbyManager) + " active at a time.");
            }

            LobbySettings.Manager = this;
            LobbySettings.Initalize();

            LobbySettings.OnGameLobbyJoinRequest.AddListener(OnGameLobbyJoinRequest.Invoke);
            LobbySettings.OnLobbyMatchList.AddListener(OnLobbyMatchList.Invoke);
            LobbySettings.OnLobbyCreated.AddListener(OnLobbyCreated.Invoke);
            LobbySettings.OnLobbyExit.AddListener(OnLobbyExit.Invoke);
            LobbySettings.OnLobbyEnter.AddListener(OnLobbyEnter.Invoke);
            LobbySettings.OnGameServerSet.AddListener(OnGameServerSet.Invoke);
            LobbySettings.OnLobbyChatUpdate.AddListener(OnLobbyChatUpdate.Invoke);
            LobbySettings.QuickMatchFailed.AddListener(QuickMatchFailed.Invoke);
            LobbySettings.SearchStarted.AddListener(SearchStarted.Invoke);
            LobbySettings.OnChatMessageReceived.AddListener(OnChatMessageReceived.Invoke);
            LobbySettings.ChatMemberStateChangeEntered.AddListener(ChatMemberStateChangeEntered.Invoke);
            LobbySettings.ChatMemberStateChangeLeft.AddListener(ChatMemberStateChangeLeft.Invoke);
            LobbySettings.ChatMemberStateChangeDisconnected.AddListener(ChatMemberStateChangeDisconnected.Invoke);
            LobbySettings.ChatMemberStateChangeKicked.AddListener(ChatMemberStateChangeKicked.Invoke);
            LobbySettings.ChatMemberStateChangeBanned.AddListener(ChatMemberStateChangeBanned.Invoke);
        }

        private void OnDestroy()
        {
            try
            {
                if (LobbySettings != null && LobbySettings.Manager == (ISteamworksLobbyManager)this)
                {
                    LobbySettings.Manager = null;

                    LobbySettings.OnGameLobbyJoinRequest.RemoveListener(OnGameLobbyJoinRequest.Invoke);
                    LobbySettings.OnLobbyMatchList.RemoveListener(OnLobbyMatchList.Invoke);
                    LobbySettings.OnLobbyCreated.RemoveListener(OnLobbyCreated.Invoke);
                    LobbySettings.OnLobbyExit.RemoveListener(OnLobbyExit.Invoke);
                    LobbySettings.OnLobbyEnter.RemoveListener(OnLobbyEnter.Invoke);
                    LobbySettings.OnGameServerSet.RemoveListener(OnGameServerSet.Invoke);
                    LobbySettings.OnLobbyChatUpdate.RemoveListener(OnLobbyChatUpdate.Invoke);
                    LobbySettings.QuickMatchFailed.RemoveListener(QuickMatchFailed.Invoke);
                    LobbySettings.SearchStarted.RemoveListener(SearchStarted.Invoke);
                    LobbySettings.OnChatMessageReceived.RemoveListener(OnChatMessageReceived.Invoke);
                    LobbySettings.ChatMemberStateChangeEntered.RemoveListener(ChatMemberStateChangeEntered.Invoke);
                    LobbySettings.ChatMemberStateChangeLeft.RemoveListener(ChatMemberStateChangeLeft.Invoke);
                    LobbySettings.ChatMemberStateChangeDisconnected.RemoveListener(ChatMemberStateChangeDisconnected.Invoke);
                    LobbySettings.ChatMemberStateChangeKicked.RemoveListener(ChatMemberStateChangeKicked.Invoke);
                    LobbySettings.ChatMemberStateChangeBanned.RemoveListener(ChatMemberStateChangeBanned.Invoke);
                }
            }
            catch { }
        }
        #endregion

        #region Deprecated Members
        /// <summary>
        /// Depricated create lobby command.
        /// This will throw an exception if called and its use will appear as an error in your compiler.
        /// </summary>
        /// <param name="lobbyFilter"></param>
        /// <param name="lobbyName"></param>
        /// <param name="lobbyType"></param>
        [Obsolete("CreateLobby(LobbyHunterFilter lobbyFilter, string lobbyName, ELobbyType lobbyType) is deprecated, please use CreateLobby(ELobbyType lobbyType, int memberCountLimit) instead.", true)]
        public void CreateLobby(LobbyHunterFilter LobbyFilter, string LobbyName = "", ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Depricated leve lobby command.
        /// This will throw an exception if called and its use will appear as an error in your compiler.
        /// </summary>
        [Obsolete("LeaveLobby is deprecated, please use the Leave method available on the SteamLobby object to leave a specific lobby, e.g. LobbySettings.lobbies[0].Leave();", true)]
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
            LobbySettings.CreateLobby(lobbyType, memberCountLimit);
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
            LobbySettings.JoinLobby(lobbyId);
        }

        /// <summary>
        /// Searches for a matching lobby according to the provided filter data.
        /// Note that a search will only start if no search is currently running.
        /// </summary>
        /// <param name="LobbyFilter">Describes the metadata to search for in a lobby</param>
        public void FindMatch(LobbyHunterFilter LobbyFilter)
        {
            if (LobbySettings != null)
            {
                LobbySettings.FindMatch(LobbyFilter);
            }
            else
            {
                Debug.LogWarning("[HeatehnSteamLobbyManager|FindMatch] attempted to find a match while [HeathenSteamLobbyManager|LobbySettings] is null");
            }
        }

        /// <summary>
        /// Starts a staged search for a matching lobby. Search will only start if no searches are currently running.
        /// </summary>
        /// <param name="LobbyFilter"></param>
        /// <param name="autoCreate"></param>
        /// <returns>True if the search was started, false otherwise.</returns>
        public bool QuickMatch(LobbyHunterFilter LobbyFilter)
        {
            if (LobbySettings != null)
            {
                return LobbySettings.QuickMatch(LobbyFilter);
            }
            else
            {
                Debug.LogWarning("[HeatehnSteamLobbyManager|QuickMatch] attempted to quick match while [HeathenSteamLobbyManager|LobbySettings] is null");
                return false;
            }
        }

        /// <summary>
        /// Terminates a quick search process
        /// Note that lobby searches are asynchronious and result may return after the cancelation
        /// </summary>
        public void CancelQuickMatch()
        {
            if (LobbySettings != null)
            {
                LobbySettings.CancelQuickMatch();
            }
            else
            {
                Debug.LogWarning("[HeatehnSteamLobbyManager|CancelQuickMatch] attempted to cancel a quick match search while [HeathenSteamLobbyManager|LobbySettings] is null");
            }
        }

        /// <summary>
        /// Terminates a standard search
        /// Note that lobby searches are asynchronious and result may return after the cancelation
        /// </summary>
        public void CancelStandardSearch()
        {
            LobbySettings.CancelStandardSearch();
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
            LobbySettings.SendChatMessage(message);
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
            LobbySettings.SetLobbyMetadata(key, value);
        }

        /// <summary>
        /// Sets metadata for the player on the first lobby
        /// </summary>
        /// <param name="key">The key of the metadata to set</param>
        /// <param name="value">The value of the metadata to set</param>
        public void SetMemberMetadata(string key, string value)
        {
            LobbySettings.SetMemberMetadata(key, value);
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
            LobbySettings.SetLobbyGameServer();
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
        public void SetLobbyGameServer(string address, ushort port, CSteamID steamID)
        {
            LobbySettings.SetLobbyGameServer(address, port, steamID);
        }
    }
}
#endif