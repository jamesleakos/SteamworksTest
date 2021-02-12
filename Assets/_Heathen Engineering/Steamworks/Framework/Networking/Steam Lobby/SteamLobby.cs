#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Networking
{
    [Serializable]
    public class SteamLobby
    {
        public const string DataName = "name";
        public const string DataVersion = "z_heathenGameVersion";
        public const string DataReady = "z_heathenReady";
        public const string DataKick = "z_heathenKick";

        /// <summary>
        /// Loads the lobby and its related data
        /// </summary>
        /// <param name="lobbyId">The lobby to load data for</param>
        /// <remarks>
        /// <see cref="SteamLobby"/> should only be initalized for a lobby that the user is a member of e.g. on create or join of a lobby.
        /// Constructing a <see cref="SteamLobby"/> for a lobby you are not a member of will cause some data to be missing due to security on Valve's side.
        /// </remarks>
        public SteamLobby(CSteamID lobbyId)
        {
            id = lobbyId;

            var memberCount = SteamMatchmaking.GetNumLobbyMembers(id);
            for (int i = 0; i < memberCount; i++)
            {
                var memberId = SteamMatchmaking.GetLobbyMemberByIndex(id, i);
                members.Add(new SteamworksLobbyMember(id, memberId));
            }

            previousOwner = Owner;
        }

        /// <summary>
        /// The id of the lobby as reported by Steam.
        /// </summary>
        public CSteamID id;

        private SteamworksLobbyMember previousOwner = null;
        /// <summary>
        /// The current owner of the lobby.
        /// </summary>
        /// <remarks>
        /// This looks up the owner from members list and repairs the members if required (adds missing member data if needed).
        /// When setting this value it will call <see cref="SteamMatchmaking.SetLobbyOwner(CSteamID, CSteamID)"/>
        /// </remarks>
        public SteamworksLobbyMember Owner
        {
            get
            {
                var ownerId = SteamMatchmaking.GetLobbyOwner(id);
                var result = members.FirstOrDefault(p => p.userData != null && p.userData.id == ownerId);
                if (result == null)
                {
                    result = new SteamworksLobbyMember(id, ownerId);
                    members.Add(result);
                }

                return result;
            }
            set
            {
                if (value.lobbyId == id && value.userData != null)
                    SteamMatchmaking.SetLobbyOwner(id, value.userData.id);
            }
        }

        /// <summary>
        /// The id of the owner of the lobby
        /// </summary>
        /// <remarks>
        /// <para>
        /// This looks up the owner from the members list and repairs the members if required (adds missing member data if needed).
        /// When setting this value it will call <see cref="SteamMatchmaking.SetLobbyOwner(CSteamID, CSteamID)"/>
        /// </para>
        /// </remarks>
        public CSteamID OwnerId
        {
            get => Owner.userData.id;
            set
            {
                if (members.Any(p => p.userData != null && p.userData.id == value))
                    SteamMatchmaking.SetLobbyOwner(id, value);
            }
        }

        /// <summary>
        /// The member data for this user
        /// </summary>
        /// <remarks>
        /// This looks up the users record in the members list and repairs the list if the entry is missing.
        /// This does confirm that the user is a member of this lobby, if the use is not a member of the lobby it will return null.
        /// </remarks>
        public SteamworksLobbyMember User
        {
            get
            {
                var userId = SteamUser.GetSteamID();
                if (id == CSteamID.Nil)
                    return null;

                var result = members.FirstOrDefault(p => p.userData != null && p.userData.id == userId);
                if(result != null)
                {
                    return result;
                }
                else
                {
                    var lobbyMemberCount = SteamMatchmaking.GetNumLobbyMembers(id);
                    for (int i = 0; i < lobbyMemberCount; i++)
                    {
                        var memberId = SteamMatchmaking.GetLobbyMemberByIndex(id, i);
                        if (memberId == userId)
                        {
                            result = new SteamworksLobbyMember(id, userId);
                            members.Add(result);
                            return result;
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// The collection of all members of this lobby including the owner of the lobby.
        /// </summary>
        /// <remarks>
        /// This should never be set from code, it is updated via Steam callbacks and contains the <see cref="SteamUserData"/> and metadata for each member.
        /// </remarks>
        public readonly List<SteamworksLobbyMember> members = new List<SteamworksLobbyMember>();
        
        #region Events
        [HideInInspector]
        public UnityEvent LobbyDataUpdateFailed = new UnityEvent();
        [HideInInspector]
        public UnityLobbyEvent OnExitLobby = new UnityLobbyEvent();
        [HideInInspector]
        public SteamworksSteamIDEvent OnKickedFromLobby = new SteamworksSteamIDEvent();
        /// <summary>
        /// Occures when a request to join the lobby has been recieved such as through Steam's invite friend dialog in the Steam Overlay
        /// </summary>
        [HideInInspector]
        public UnityGameLobbyJoinRequestedEvent OnGameLobbyJoinRequest = new UnityGameLobbyJoinRequestedEvent();
        /// <summary>
        /// Occures when the owner of the currently tracked lobby changes
        /// </summary>
        [HideInInspector]
        public SteamworksLobbyMemberEvent OnOwnershipChange = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when a member joins the lobby
        /// </summary>
        [HideInInspector]
        public SteamworksLobbyMemberEvent OnMemberJoined = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when a member leaves the lobby
        /// </summary>
        [HideInInspector]
        public SteamworksLobbyMemberEvent OnMemberLeft = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when Steam metadata for a member changes
        /// </summary>
        [HideInInspector]
        public SteamworksLobbyMemberEvent OnMemberDataChanged = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when lobby metadata changes
        /// </summary>
        [HideInInspector]
        public UnityEvent OnLobbyDataChanged = new UnityEvent();
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
        /// Get or set the lobby name
        /// </summary>
        /// <remarks>
        /// <para>
        /// The lobby name is a metadata field whoes key is "name". Setting this field will update the lobby metadata accordinly and this update will be reflected to all members.
        /// Only the owner of the lobby can set this value.
        /// </para>
        /// </remarks>
        public string Name 
        { 
            get => this[DataName]; 
            set => this[DataName] = value; 
        }

        /// <summary>
        /// Gets or sets the version of the game the lobby is configured for ... this should match the owners version
        /// </summary>
        public string GameVersion
        {
            get => this[DataVersion];
            set => this[DataVersion] = value;
        }

        /// <summary>
        /// The current limit for member count
        /// </summary>
        public int MemberCountLimit
        {
            get => SteamMatchmaking.GetLobbyMemberLimit(id);
            set => SteamMatchmaking.SetLobbyMemberLimit(id, value);
        }

        /// <summary>
        /// Returns the number of users that are members of this lobby
        /// </summary>
        public int MemberCount => members.Count;

        private bool p_joinable = true;
        /// <summary>
        /// <para>
        /// Sets whether or not a lobby is joinable by other players. This always defaults to enabled for a new lobby.
        /// If joining is disabled, then no players can join, even if they are a friend or have been invited.
        /// Lobbies with joining disabled will not be returned from a lobby search.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is only accruate on the lobby owner's system and may not be accurate if the owner has changed since it was set.
        /// In general its advisable that you have a new owner set Joinable to the desired value when they are made owner doing so will cause the value to sync for that user.
        /// </para>
        /// </remarks>
        public bool Joinable
        {
            get => p_joinable;
            set
            {
                if (SteamMatchmaking.SetLobbyJoinable(id, value))
                    p_joinable = value;
            }
        }

        /// <summary>
        /// The game server information stored against the lobby
        /// </summary>
        /// <remarks>
        /// <para>
        /// This data is set when the host calls <see cref="SetLobbyGameServer"/> or one of its variants. Uppon calling <see cref="SetLobbyGameServer"/> the Valve backend will raise <see cref="OnGameServerSet"/> for all members other than the host the paramiter of which also contains server data.
        /// The typical use case of this field is when a member has join a persistent lobby after the game server has been started.
        /// </para>
        /// </remarks>
        public LobbyGameServerInformation GameServer { get; private set; }

        /// <summary>
        /// Is the user the host of this lobby
        /// </summary>
        /// <remarks>
        /// <para>
        /// Calls <see cref="SteamMatchmaking.GetLobbyOwner(CSteamID)"/> and compares the results to <see cref="SteamUser.GetSteamID()"/>.
        /// This returns true if the provided lobby ID is a legitimate ID and if Valve indicates that the lobby has members and if the owner of the lobby is the current player.
        /// </para>
        /// </remarks>
        public bool IsHost
        {
            get
            {
                return SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(id);
            }
        }

        /// <summary>
        /// Does this lobby have a game server registered to it
        /// </summary>
        /// <remarks>
        /// <para>
        /// Calls <see cref="SteamMatchmaking.GetLobbyGameServer(CSteamID, out uint, out ushort, out CSteamID)"/> and cashes the data to <see cref="GameServer"/>.
        /// It is not usually nessisary to check this value since the set game server callback from Steam will automatically update these values if the user was connected to the lobby when the set game server data was called.
        /// </para>
        /// </remarks>
        public bool HasGameServer
        {
            get
            {
                uint ipBuffer;
                ushort portBuffer;
                CSteamID steamIdBuffer;
                var result = SteamMatchmaking.GetLobbyGameServer(id, out ipBuffer, out portBuffer, out steamIdBuffer);

                GameServer = new LobbyGameServerInformation()
                {
                    ipAddress = ipBuffer,
                    port = portBuffer,
                    serverId = steamIdBuffer
                };

                return result;
            }
        }

        /// <summary>
        /// Read and write metadata values to the lobby
        /// </summary>
        /// <param name="metadataKey">The key of the value to be read or writen</param>
        /// <returns>The value of the key if any otherwise returns and empty string.</returns>
        public string this[string metadataKey]
        {
            get
            {
                return SteamMatchmaking.GetLobbyData(id, metadataKey);
            }
            set
            {
                SteamMatchmaking.SetLobbyData(id, metadataKey, value);
            }
        }

        /// <summary>
        /// Returns the number of metadata keys set on the lobby
        /// </summary>
        /// <returns></returns>
        public int GetMetadataCount()
        {
            return SteamMatchmaking.GetLobbyDataCount(id);
        }

        /// <summary>
        /// Gets the dictionary of metadata values assigned to this lobby.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetMetadataEntries()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            var count = SteamMatchmaking.GetLobbyDataCount(id);

            for (int i = 0; i < count; i++)
            {
                string key;
                string value;
                SteamMatchmaking.GetLobbyDataByIndex(id, i, out key, Constants.k_nMaxLobbyKeyLength, out value, Constants.k_cubChatMetadataMax);
                result.Add(key, value);
            }

            return result;
        }

        /// <summary>
        /// Returns true if all of the players 'IsReady' is true
        /// </summary>
        /// <remarks>
        /// <para>
        /// This can be used to determin if the players are ready to play the game.
        /// </para>
        /// </remarks>
        public bool AllPlayersReady
        {
            get
            {
                //If we have any that are not ready then return false ... else return true
                return members.Any( p => !p.IsReady) ? false : true;
            }
        }

        /// <summary>
        /// Returns true if all of the players 'IsReady' is false
        /// </summary>
        /// <remarks>
        /// <para>
        /// This can be used to determin if all players have reset the ready flag such as when some change is made after a previous ready check had already passed.
        /// </para>
        /// </remarks>
        public bool AllPlayersNotReady
        {
            get
            {
                //If we have any that are not ready then return false ... else return true
                return members.Any(p => p.IsReady) ? false : true;
            }
        }

        #region Callback Handlers
        internal void HandleLobbyGameCreated(LobbyGameCreated_t param)
        {
            GameServer = new LobbyGameServerInformation
            {
                ipAddress = param.m_unIP,
                port = param.m_usPort,
                serverId = new CSteamID(param.m_ulSteamIDGameServer)
            };

            OnGameServerSet.Invoke(param);
        }

        internal void HandleLobbyChatUpdate(LobbyChatUpdate_t pCallback)
        {
            if (id.m_SteamID != pCallback.m_ulSteamIDLobby)
                return;

            if (pCallback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft)
            {
                var memberId = new CSteamID(pCallback.m_ulSteamIDUserChanged);
                var member = members.FirstOrDefault(p => p.userData != null && p.userData.id == memberId);

                if (member != null)
                {
                    members.Remove(member);
                    OnMemberLeft.Invoke(member);
                    ChatMemberStateChangeLeft.Invoke(member.userData);
                }
            }
            else if (pCallback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered)
            {
                var memberId = new CSteamID(pCallback.m_ulSteamIDUserChanged);
                var member = members.FirstOrDefault(p => p.userData != null && p.userData.id == memberId);

                if (member == null)
                {
                    member = new SteamworksLobbyMember(id, memberId);
                    members.Add(member);
                    OnMemberJoined.Invoke(member);
                }

                ChatMemberStateChangeEntered.Invoke(member);
            }
            else if (pCallback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
            {
                var memberId = new CSteamID(pCallback.m_ulSteamIDUserChanged);
                var member = members.FirstOrDefault(p => p.userData != null && p.userData.id == memberId);

                if (member != null)
                {
                    members.Remove(member);
                    OnMemberLeft.Invoke(member);
                    ChatMemberStateChangeDisconnected.Invoke(member.userData);
                }
            }
            else if (pCallback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeKicked)
            {
                var memberId = new CSteamID(pCallback.m_ulSteamIDUserChanged);
                var member = members.FirstOrDefault(p => p.userData != null && p.userData.id == memberId);

                if (member != null)
                {
                    members.Remove(member);
                    OnMemberLeft.Invoke(member);
                    ChatMemberStateChangeKicked.Invoke(member.userData);
                }
            }
            else if (pCallback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeBanned)
            {
                var memberId = new CSteamID(pCallback.m_ulSteamIDUserChanged);
                var member = members.FirstOrDefault(p => p.userData != null && p.userData.id == memberId);

                if (member != null)
                {
                    members.Remove(member);
                    OnMemberLeft.Invoke(member);
                    ChatMemberStateChangeBanned.Invoke(member.userData);
                }
            }

            OnLobbyChatUpdate.Invoke(pCallback);

            var currentOwner = Owner;
            if(previousOwner != currentOwner)
            {
                previousOwner = currentOwner;
                OnOwnershipChange.Invoke(currentOwner);
            }
        }

        internal void HandleLobbyDataUpdate(LobbyDataUpdate_t param)
        {
            var askedToLeave = false;

            if (param.m_bSuccess == 0)
            {
                LobbyDataUpdateFailed.Invoke();
                return;
            }

            if (param.m_ulSteamIDLobby == param.m_ulSteamIDMember)
            {
                if(SteamMatchmaking.GetLobbyData(id, DataKick).Contains("[" + SteamUser.GetSteamID().m_SteamID.ToString() + "]"))
                {
                    Debug.Log("User has been kicked from the lobby.");
                    askedToLeave = true;
                }

                OnLobbyDataChanged.Invoke();
            }
            else
            {
                var userId = new CSteamID(param.m_ulSteamIDMember);

                var member = members.FirstOrDefault(p => p.userData != null && p.userData.id == userId);
                if (member == null)
                {
                    member = new SteamworksLobbyMember(id, userId);
                    members.Add(member);
                    OnMemberJoined.Invoke(member);
                }

                OnMemberDataChanged.Invoke(member);
            }

            if (askedToLeave)
            {
                var id = this.id;
                Leave();
                OnKickedFromLobby.Invoke(id);
            }
            else
            {
                var currentOwner = Owner;
                if (previousOwner != currentOwner)
                {
                    previousOwner = currentOwner;
                    OnOwnershipChange.Invoke(currentOwner);
                }
            }
        }

        internal LobbyChatMessageData HandleLobbyChatMessage(LobbyChatMsg_t pCallback)
        {
            var subjectLobby = (CSteamID)pCallback.m_ulSteamIDLobby;
            if (subjectLobby != id)
                return null;

            CSteamID SteamIDUser;
            byte[] Data = new byte[4096];
            EChatEntryType ChatEntryType;
            int ret = SteamMatchmaking.GetLobbyChatEntry(subjectLobby, (int)pCallback.m_iChatID, out SteamIDUser, Data, Data.Length, out ChatEntryType);
            byte[] truncated = new byte[ret];
            Array.Copy(Data, truncated, ret);

            LobbyChatMessageData record = new LobbyChatMessageData
            {
                sender = members.FirstOrDefault(p => p.userData.id == SteamIDUser),
                message = System.Text.Encoding.UTF8.GetString(truncated),
                recievedTime = DateTime.Now,
                chatEntryType = ChatEntryType,
                lobby = this,
            };

            OnChatMessageReceived.Invoke(record);

            return record;
        }
        #endregion

        /// <summary>
        /// Leaves the current lobby if any
        /// </summary>
        public void Leave()
        {
            if (id == CSteamID.Nil)
                return;

            try
            {
                SteamMatchmaking.LeaveLobby(id);
            }
            catch { }

            OnExitLobby.Invoke(this);

            id = CSteamID.Nil;
            members.Clear();
        }

        public bool DeleteLobbyData(string dataKey)
        {
            return SteamMatchmaking.DeleteLobbyData(id, dataKey);
        }

        public bool InviteUserToLobby(CSteamID targetUser)
        {
            return SteamMatchmaking.InviteUserToLobby(id, targetUser);
        }

        public bool SendChatMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return false;

            byte[] MsgBody = System.Text.Encoding.UTF8.GetBytes(message);
            return SteamMatchmaking.SendLobbyChatMsg(id, MsgBody, MsgBody.Length);
        }

        /// <summary>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steam ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <see href="https://partner.steamgames.com/doc/api/ISteamMatchmaking#SetLobbyGameServer">SetLobbyGameServer</see> in Valve's documentaiton.
        /// This will update the game server settings for the lobby notifying all members of the new data.
        /// This should only be called when you are ready for the members to join the server.
        /// </para>
        /// </remarks>
        /// <param name="address">The IP address of the game server as typicall string address "127.0.0.1"</param>
        /// <param name="port">The port of the game server</param>
        /// <param name="gameServerId">The steam ID of the game server, if this is P2P then this would be the host's CSteamID</param>
        public void SetGameServer(string address, ushort port, CSteamID gameServerId)
        {
            GameServer = new LobbyGameServerInformation
            {
                port = port,
                StringAddress = address,
                serverId = gameServerId
            };

            SteamMatchmaking.SetLobbyGameServer(id, GameServer.ipAddress, port, gameServerId);
        }

        /// <summary>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steam ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <see href="https://partner.steamgames.com/doc/api/ISteamMatchmaking#SetLobbyGameServer">SetLobbyGameServer</see> in Valve's documentaiton.
        /// This will update the game server settings for the lobby notifying all members of the new data.
        /// This should only be called when you are ready for the members to join the server.
        /// </para>
        /// </remarks>
        /// <param name="address">The IP address of the game server as typicall string address "127.0.0.1"</param>
        /// <param name="port">The port of the game server</param>
        public void SetGameServer(string address, ushort port)
        {
            GameServer = new LobbyGameServerInformation
            {
                port = port,
                StringAddress = address,
                serverId = CSteamID.Nil
            };

            SteamMatchmaking.SetLobbyGameServer(id, GameServer.ipAddress, port, CSteamID.Nil);
        }

        /// <summary>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steam ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <see href="https://partner.steamgames.com/doc/api/ISteamMatchmaking#SetLobbyGameServer">SetLobbyGameServer</see> in Valve's documentaiton.
        /// This will update the game server settings for the lobby notifying all members of the new data.
        /// This should only be called when you are ready for the members to join the server.
        /// </para>
        /// </remarks>
        /// <param name="gameServerId">The steam ID of the game server, if this is P2P then this would be the host's CSteamID</param>
        public void SetGameServer(CSteamID gameServerId)
        {
            GameServer = new LobbyGameServerInformation
            {
                port = 0,
                ipAddress = 0,
                serverId = gameServerId
            };

            SteamMatchmaking.SetLobbyGameServer(id, 0, 0, gameServerId);
        }

        /// <summary>
        /// <para>
        /// This overload uses the lobby owner's CSteamID as the server ID which is typical of P2P session.
        /// </para>
        /// <para>
        /// Sets the game server associated with the lobby.
        /// This can only be set by the owner of the lobby.
        /// Either the IP/Port or the Steam ID of the game server must be valid, depending on how you want the clients to be able to connect.
        /// A LobbyGameCreated_t callback will be sent to all players in the lobby, usually at this point, the users will join the specified game server.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <see href="https://partner.steamgames.com/doc/api/ISteamMatchmaking#SetLobbyGameServer">SetLobbyGameServer</see> in Valve's documentaiton.
        /// This will update the game server settings for the lobby notifying all members of the new data.
        /// This should only be called when you are ready for the members to join the server.
        /// </para>
        /// </remarks>
        public void SetGameServer()
        {
            GameServer = new LobbyGameServerInformation
            {
                port = 0,
                ipAddress = 0,
                serverId = SteamUser.GetSteamID()
            };

            SteamMatchmaking.SetLobbyGameServer(id, 0, 0, GameServer.serverId);
        }

        /// <summary>
        /// <para>
        /// Updates what type of lobby this is.
        /// This is also set when you create the lobby with CreateLobby.
        /// This can only be set by the owner of the lobby.
        /// </para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// See <see href="https://partner.steamgames.com/doc/api/ISteamMatchmaking#SetLobbyType">SetLobbyType</see> in Valve's documentaiton.
        /// </para>
        /// </remarks>
        public bool SetLobbyType(ELobbyType type)
        {
            return SteamMatchmaking.SetLobbyType(id, type);
        }

        /// <summary>
        /// Marks the user to be removed
        /// </summary>
        /// <param name="memberId"></param>
        /// <remarks>
        /// This creates an entry in the metadata named z_heathenKick which contains a string array of Ids of users that should leave the lobby.
        /// When users detect their ID in the string they will automatically leave the lobby on leaving the lobby the users ID will be removed from the array.
        /// </remarks>
        public void KickMember(CSteamID memberId)
        {
            if (!IsHost)
            {
                Debug.LogError("Only the host of a lobby can kick a member from it.");
                return;
            }

            if (memberId.m_SteamID == SteamUser.GetSteamID().m_SteamID)
            {
                Leave();
                OnKickedFromLobby.Invoke(SteamUser.GetSteamID());
                return;
            }
            else
            {
                Debug.Log("Marking " + memberId.m_SteamID + " for removal");
            }

            var kickList = SteamMatchmaking.GetLobbyData(id, DataKick);

            if (kickList == null)
                kickList = string.Empty;

            if (!kickList.Contains("[" + memberId.ToString() + "]"))
                kickList += "[" + memberId.ToString() + "]";

            SteamMatchmaking.SetLobbyData(id, DataKick, kickList);
        }
    }

}
#endif