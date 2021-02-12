#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS 
using Steamworks;

namespace HeathenEngineering.SteamApi.Networking
{
    public interface ISteamworksLobbyManager
    {
        /// <summary>
        /// Checks if a search is currently running
        /// </summary>
        bool IsSearching { get; }

        bool IsQuickSearching { get; }

        void CreateLobby(LobbyHunterFilter LobbyFilter, string LobbyName = "", ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic);

        void JoinLobby(CSteamID lobbyId);

        void LeaveLobby();

        void FindMatch(LobbyHunterFilter LobbyFilter);

        /// <summary>
        /// Starts a staged search for a matching lobby. Search will only start if no searches are currently running.
        /// </summary>
        /// <param name="LobbyFilter"></param>
        /// <param name="autoCreate"></param>
        /// <returns>True if the search was started, false otherwise.</returns>
        bool QuickMatch(LobbyHunterFilter LobbyFilter);

        void CancelQuickMatch();

        void CancelStandardSearch();

        void SendChatMessage(string message);

        void SetLobbyMetadata(string key, string value);

        void SetMemberMetadata(string key, string value);

        void SetLobbyGameServer();

        void SetLobbyGameServer(string address, ushort port, CSteamID steamID);
    }
}
#endif