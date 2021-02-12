#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using System;

namespace HeathenEngineering.SteamApi.Networking
{
    public class SteamworksLobbyMember
    {
        /// <summary>
        /// The lobby this user is a member of
        /// </summary>
        public CSteamID lobbyId;
        /// <summary>
        /// Contains the personal/profile information about his user
        /// </summary>
        public SteamUserData userData;

        #region Deprecated Members
        /// <summary>
        /// No longer used.
        /// Use of this member will throw an error on compilation.
        /// </summary>
        /// <remarks>
        /// To set or get metadata values for a member use the string indexer e.g.
        /// <code>
        /// myLobbyMember["metadataKey"] = "This sets the value of the key metadataKey.";
        /// </code>
        /// Valve does not provide a means to fetch all set values nor iterate over values by index for member metadata as it does for Lobby metadata. 
        /// There for you must know the key of the data you are looking for. Attempting to get a field that doesn't exist simply results in an empty string and will not cause an out of bounds exception.
        /// <para>
        /// The previous system had you set possible metadata keys on the LobbySettings object and used that to assume keys available on the member. As the system now supports multiple lobbies at a time this is no longer appropreat on the lobby settings object.
        /// You can create your own index of possible keys and use the indexer provided here to access the values.
        /// </para>
        /// </remarks>
        [Obsolete("Metadata member is no longer used on the SteamworksLobbyMember object, please use the string indexer [string metadataKey] to access a specific metadata field.", true)]
        public SteamworksLobbyMetadata Metadata { get; }
        #endregion

        /// <summary>
        /// Read and write metadata values to the lobby
        /// </summary>
        /// <param name="metadataKey">The key of the value to be read or writen</param>
        /// <returns>The value of the key if any otherwise returns and empty string.</returns>
        public string this[string metadataKey]
        {
            get
            {
                return SteamMatchmaking.GetLobbyMemberData(lobbyId, userData.id, metadataKey);
            }
            set
            {
                SteamMatchmaking.SetLobbyMemberData(lobbyId, metadataKey, value);
            }
        }

        public SteamworksLobbyMember(CSteamID lobbyId, CSteamID userId)
        {
            this.lobbyId = lobbyId;
            userData = SteamSettings.current.client.GetUserData(userId);
        }

        public bool IsReady
        {
            get => this[SteamLobby.DataReady] == "true";
            set => this[SteamLobby.DataReady] = value.ToString().ToLower();
        }

        public string GameVersion
        {
            get => this[SteamLobby.DataVersion];
            set => this[SteamLobby.DataVersion] = value;
        }
    }
}
#endif