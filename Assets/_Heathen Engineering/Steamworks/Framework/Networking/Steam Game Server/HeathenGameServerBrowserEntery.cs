#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS && MIRROR
using HeathenEngineering.SteamApi.Foundation;
using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Networking
{
    [Serializable]
    public class HeathenGameServerBrowserEntery
    {
        public CSteamID serverID;
        public string serverName;
        public string serverDescription;
        public string mapName;
        public bool isPasswordProtected;
        public bool isVAC;
        public int maxPlayerCount;
        public int currentPlayerCount;
        public int botPlayers;
        public int ping;
        public int serverVersion;
        public string tags;
        public servernetadr_t address;
        public DateTime lastTimePlayed;
        public List<StringKeyValuePair> rules;
        public List<ServerPlayerEntry> players;

        public UnityEvent DataUpdated;

        public void FromGameServerItem(gameserveritem_t item)
        {
            serverID = item.m_steamID;
            serverName = item.GetServerName();
            serverDescription = item.GetGameDescription();
            mapName = item.GetMap();
            isPasswordProtected = item.m_bPassword;
            isVAC = item.m_bSecure;
            maxPlayerCount = item.m_nMaxPlayers;
            currentPlayerCount = item.m_nPlayers;
            botPlayers = item.m_nBotPlayers;
            ping = item.m_nPing;
            serverVersion = item.m_nServerVersion;
            tags = item.GetGameTags();
            address = item.m_NetAdr;
            lastTimePlayed = SteamUtilities.ConvertUnixDate(item.m_ulTimeLastPlayed);
        }

        /// <summary>
        /// Join the indicated server if we are not already part of a server
        /// </summary>
        /// <returns></returns>
        public bool JoinServer()
        {
            if (!NetworkManager.singleton.isNetworkActive)
            {
                NetworkManager.singleton.networkAddress = serverID.m_SteamID.ToString();
                NetworkManager.singleton.StartClient();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Leave the current server e.g. Stop Client Networking
        /// </summary>
        public void LeaveServer()
        {
            NetworkManager.singleton.StopClient();
        }

        /// <summary>
        /// Switch to the indicated server i.e. leave the current server if any and join the indicated one
        /// </summary>
        public void SwitchServer()
        {
            if (NetworkManager.singleton.isNetworkActive)
                NetworkManager.singleton.StopClient();

            NetworkManager.singleton.networkAddress = serverID.m_SteamID.ToString();
            NetworkManager.singleton.StartClient();
        }
    }
}
#endif