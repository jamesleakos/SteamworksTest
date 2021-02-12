#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS && MIRROR
using HeathenEngineering.SteamApi.Foundation;
using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Networking
{
    [RequireComponent(typeof(SteamworksFoundationManager))]
    [DisallowMultipleComponent]
    public class HeathenGameServerBrowser : MonoBehaviour
    {
        public SteamSettings steamSettings;
        public NetworkManager networkManager;

        public List<MatchMakingKeyValuePair_t> filter = new List<MatchMakingKeyValuePair_t>();

        public List<HeathenGameServerBrowserEntery> InternetServers;
        public List<HeathenGameServerBrowserEntery> FavoritesServers;
        public List<HeathenGameServerBrowserEntery> FriendsServers;
        public List<HeathenGameServerBrowserEntery> LANServers;
        public List<HeathenGameServerBrowserEntery> SpectatorServers;
        public List<HeathenGameServerBrowserEntery> HistoryServers;

        public UnityEvent InternetServerListUpdated;
        public UnityEvent FavoriteServerListUpdated;
        public UnityEvent FriendsServerListUpdated;
        public UnityEvent LANServerListUpdated;
        public UnityEvent SpectatorServerListUpdated;
        public UnityEvent HistoryServerListUpdated;
        public UnityEvent ServerRefreshFailed;
        public UnityEvent ServerRefreshCompleted;
        public ServerQueryEvent RulesQueryCompleted;
        public ServerQueryEvent RulesQueryFailed;
        public ServerQueryEvent PingCompleted;
        public ServerQueryEvent PingFailed;
        public ServerQueryEvent PlayerListQueryCompleted;
        public ServerQueryEvent PlayerListQueryFailed;

        private HServerListRequest m_ServerListRequest = HServerListRequest.Invalid;
        private HServerQuery m_ServerQuery = HServerQuery.Invalid;
        private HeathenGameServerBrowserEntery queryTarget;
        private List<StringKeyValuePair> rulesListWorking = new List<StringKeyValuePair>();
        private List<ServerPlayerEntry> playersListWorking = new List<ServerPlayerEntry>();
        
        private ISteamMatchmakingServerListResponse m_ServerListResponse;
        private ISteamMatchmakingPingResponse m_PingResponse;
        private ISteamMatchmakingPlayersResponse m_PlayersResponse;
        private ISteamMatchmakingRulesResponse m_RulesResponse;

        private enum SearchType
        {
            Internet,
            Friends,
            Favorites,
            LAN,
            Spectator,
            History
        }
        private SearchType searchType = SearchType.Internet;

        public void OnEnable()
        {
            m_ServerListResponse = new ISteamMatchmakingServerListResponse(OnServerResponded, OnServerFailedToRespond, OnRefreshComplete);
            m_PingResponse = new ISteamMatchmakingPingResponse(OnServerRespondedPing, OnServerFailedToRespondPing);
            m_PlayersResponse = new ISteamMatchmakingPlayersResponse(OnAddPlayerToList, OnPlayersFailedToRespond, OnPlayersRefreshComplete);
            m_RulesResponse = new ISteamMatchmakingRulesResponse(OnRulesResponded, OnRulesFailedToRespond, OnRulesRefreshComplete);
        }

        private void OnDisable()
        {
            ReleaseRequest();
            CancelServerQuery();
        }

        /// <summary>
        /// Join the indicated server if we are not already part of a server
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public bool JoinServer(CSteamID steamId)
        {
            if (!networkManager.isNetworkActive)
            {
                networkManager.networkAddress = steamId.m_SteamID.ToString();
                networkManager.StartClient();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Leave the current server e.g. Stop Client Networking
        /// </summary>
        public void LeaveServer()
        {
            networkManager.StopClient();
        }

        /// <summary>
        /// Switch to the indicated server i.e. leave the current server if any and join the indicated one
        /// </summary>
        /// <param name="steamId"></param>
        public void SwitchServer(CSteamID steamId)
        {
            if (networkManager.isNetworkActive)
                NetworkManager.singleton.StopClient();

            networkManager.networkAddress = steamId.m_SteamID.ToString();
            networkManager.StartClient();
        }

        /// <summary>
        /// Clears the filter list
        /// </summary>
        public void FilterClear()
        {
            filter.Clear();
        }

        /// <summary>
        /// Adds a filter to the list
        /// </summary>
        /// <param name="key">The key to add</param>
        /// <param name="value">The value to add</param>
        public void FilterAdd(string key, string value)
        {
            filter.Add(new MatchMakingKeyValuePair_t() { m_szKey = key, m_szValue = value });
        }

        /// <summary>
        /// Removes a filter from the list based on its key
        /// </summary>
        /// <param name="key">The key to remove ... all occurances will be removed</param>
        public void FilterRemove(string key)
        {
            filter.RemoveAll(p => p.m_szKey == key);
        }

        public void RefreshInternetServers()
        {
            ReleaseRequest();
            searchType = SearchType.Internet;

            if(m_ServerListResponse == null)
                m_ServerListResponse = new ISteamMatchmakingServerListResponse(OnServerResponded, OnServerFailedToRespond, OnRefreshComplete);

            m_ServerListRequest = SteamMatchmakingServers.RequestInternetServerList(steamSettings.applicationId, filter.ToArray(), System.Convert.ToUInt32(filter.Count), m_ServerListResponse);
        }

        public void RefreshFavoriteServers()
        {
            ReleaseRequest();
            searchType = SearchType.Favorites;

            if (m_ServerListResponse == null)
                m_ServerListResponse = new ISteamMatchmakingServerListResponse(OnServerResponded, OnServerFailedToRespond, OnRefreshComplete);

            m_ServerListRequest = SteamMatchmakingServers.RequestFavoritesServerList(steamSettings.applicationId, filter.ToArray(), System.Convert.ToUInt32(filter.Count), m_ServerListResponse);
        }

        public void RefreshFriendServers()
        {
            ReleaseRequest();
            searchType = SearchType.Friends;

            if (m_ServerListResponse == null)
                m_ServerListResponse = new ISteamMatchmakingServerListResponse(OnServerResponded, OnServerFailedToRespond, OnRefreshComplete);

            m_ServerListRequest = SteamMatchmakingServers.RequestFriendsServerList(steamSettings.applicationId, filter.ToArray(), System.Convert.ToUInt32(filter.Count), m_ServerListResponse);
        }

        public void RefreshLANServers()
        {
            ReleaseRequest();
            searchType = SearchType.LAN;

            if (m_ServerListResponse == null)
                m_ServerListResponse = new ISteamMatchmakingServerListResponse(OnServerResponded, OnServerFailedToRespond, OnRefreshComplete);

            m_ServerListRequest = SteamMatchmakingServers.RequestLANServerList(steamSettings.applicationId, m_ServerListResponse);
        }

        public void RefreshSpectatorServers()
        {
            ReleaseRequest();
            searchType = SearchType.Spectator;

            if (m_ServerListResponse == null)
                m_ServerListResponse = new ISteamMatchmakingServerListResponse(OnServerResponded, OnServerFailedToRespond, OnRefreshComplete);

            m_ServerListRequest = SteamMatchmakingServers.RequestSpectatorServerList(steamSettings.applicationId, filter.ToArray(), System.Convert.ToUInt32(filter.Count), m_ServerListResponse);
        }

        public void RefreshHistoryServers()
        {
            ReleaseRequest();
            searchType = SearchType.History;

            if (m_ServerListResponse == null)
                m_ServerListResponse = new ISteamMatchmakingServerListResponse(OnServerResponded, OnServerFailedToRespond, OnRefreshComplete);

            m_ServerListRequest = SteamMatchmakingServers.RequestHistoryServerList(steamSettings.applicationId, filter.ToArray(), System.Convert.ToUInt32(filter.Count), m_ServerListResponse);
        }

        public void RefreshServerRules(HeathenGameServerBrowserEntery target)
        {
            CancelServerQuery();
            queryTarget = target;
            rulesListWorking.Clear();
            m_ServerQuery = SteamMatchmakingServers.ServerRules(target.address.GetIP(), target.address.GetQueryPort(), m_RulesResponse);
        }

        public void RefreshServerData(HeathenGameServerBrowserEntery target)
        {
            PingServer(target);
        }

        public void PingServer(HeathenGameServerBrowserEntery target)
        {
            CancelServerQuery();
            queryTarget = target;

            m_ServerQuery = SteamMatchmakingServers.PlayerDetails(target.address.GetIP(), target.address.GetQueryPort(), m_PlayersResponse);
        }

        public void RefreshServerPlayerList(HeathenGameServerBrowserEntery target)
        {
            CancelServerQuery();
            queryTarget = target;
            playersListWorking.Clear();
            m_ServerQuery = SteamMatchmakingServers.PlayerDetails(target.address.GetIP(), target.address.GetQueryPort(), m_PlayersResponse);
        }

        private void ReleaseRequest()
        {
            if (m_ServerListRequest != HServerListRequest.Invalid)
            {
                SteamMatchmakingServers.ReleaseRequest(m_ServerListRequest);
                m_ServerListRequest = HServerListRequest.Invalid;
                print("SteamMatchmakingServers.ReleaseRequest(m_ServerListRequest)");
            }
        }

        private void CancelServerQuery()
        {
            if (m_ServerQuery != HServerQuery.Invalid)
            {
                SteamMatchmakingServers.CancelServerQuery(m_ServerQuery);
                m_ServerQuery = HServerQuery.Invalid;
                print("SteamMatchmakingServers.CancelServerQuery(m_ServerQuery)");
            }
        }

        private void OnRulesRefreshComplete()
        {
            CancelServerQuery();
            queryTarget.rules = rulesListWorking;
            RulesQueryCompleted.Invoke(queryTarget);
        }

        private void OnRulesFailedToRespond()
        {
            CancelServerQuery();
            RulesQueryFailed.Invoke(queryTarget);
        }

        private void OnRulesResponded(string pchRule, string pchValue)
        {
            rulesListWorking.Add(new StringKeyValuePair() { key = pchRule, value = pchValue });
        }

        private void OnPlayersRefreshComplete()
        {
            CancelServerQuery();
            queryTarget.players = playersListWorking;
            PlayerListQueryCompleted.Invoke(queryTarget);
        }

        private void OnPlayersFailedToRespond()
        {
            CancelServerQuery();
            PlayerListQueryFailed.Invoke(queryTarget);
        }

        private void OnAddPlayerToList(string pchName, int nScore, float flTimePlayed)
        {
            playersListWorking.Add(new ServerPlayerEntry() { name = pchName, score = nScore, timePlayed = new TimeSpan(0, 0, 0, (int)flTimePlayed, 0) });
        }

        private void OnServerFailedToRespondPing()
        {
            CancelServerQuery();
            PingFailed.Invoke(queryTarget);
        }

        private void OnServerRespondedPing(gameserveritem_t server)
        {
            CancelServerQuery();
            queryTarget.FromGameServerItem(server);
            PingCompleted.Invoke(queryTarget);
        }

        private void OnRefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response)
        {
            Debug.Log("OnRefreshComplete: " + hRequest + " - " + response);
            List<HeathenGameServerBrowserEntery> serverResults = new List<HeathenGameServerBrowserEntery>();
            var count = SteamMatchmakingServers.GetServerCount(hRequest);

            for (int i = 0; i < count; i++)
            {
                var serverItem = SteamMatchmakingServers.GetServerDetails(hRequest, i);

                if (serverItem.m_steamID.m_SteamID != 0 && serverItem.m_nAppID == steamSettings.applicationId.m_AppId)
                {
                    var entry = new HeathenGameServerBrowserEntery();
                    entry.FromGameServerItem(serverItem);
                    serverResults.Add(entry);
                }
            }
            ReleaseRequest();
            Debug.Log(serverResults.Count.ToString() + " Servers Found");

            switch (searchType)
            {
                case SearchType.Internet:
                    InternetServers.Clear();
                    InternetServers = serverResults;
                    InternetServerListUpdated.Invoke();
                    break;
                case SearchType.Favorites:
                    FavoritesServers.Clear();
                    FavoritesServers = serverResults;
                    FavoriteServerListUpdated.Invoke();
                    break;
                case SearchType.Friends:
                    FriendsServers.Clear();
                    FriendsServers = serverResults;
                    FriendsServerListUpdated.Invoke();
                    break;
                case SearchType.LAN:
                    LANServers.Clear();
                    LANServers = serverResults;
                    LANServerListUpdated.Invoke();
                    break;
                case SearchType.Spectator:
                    SpectatorServers.Clear();
                    SpectatorServers = serverResults;
                    SpectatorServerListUpdated.Invoke();
                    break;
                case SearchType.History:
                    HistoryServers.Clear();
                    HistoryServers = serverResults;
                    HistoryServerListUpdated.Invoke();
                    break;
                default:
                    break;
            }

            ServerRefreshCompleted.Invoke();
        }

        private void OnServerFailedToRespond(HServerListRequest hRequest, int iServer)
        {
            Debug.Log("OnServerFailedToRespond: " + hRequest + " - " + iServer);
            ServerRefreshFailed.Invoke();
        }

        private void OnServerResponded(HServerListRequest hRequest, int iServer)
        {
            Debug.Log("OnServerResponded: " + hRequest + " - " + iServer);
        }
    }
}
#endif