#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System.Collections.Generic;
using System;

namespace HeathenEngineering.SteamApi.Networking
{
    [Serializable]
    public class SteamLobbyLobbyList : List<LobbyHunterLobbyRecord>
    {
        public Dictionary<string, string> GetLobbyMetaData(CSteamID id)
        {
            if (this.Exists(p => p.lobbyId.m_SteamID == id.m_SteamID))
                return this.Find(p => p.lobbyId.m_SteamID == id.m_SteamID).metadata;
            else
                return new Dictionary<string, string>();
        }
    }
}
#endif
