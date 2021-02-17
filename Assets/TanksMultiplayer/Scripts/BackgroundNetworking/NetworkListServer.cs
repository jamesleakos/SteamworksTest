using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Mirror.Cloud;
using Mirror.Cloud.ListServerService;

namespace Errantastra
{
    /// <summary>
    /// List server client implementation that adds, updates and removes games from the List Server.
    /// </summary>
    [RequireComponent(typeof(NetworkManager))]
    [RequireComponent(typeof(ApiConnector))]
    public class NetworkListServer : MonoBehaviour
    {
        private static ApiConnector connector;

        /// <summary>
        /// The name a game is listed as. By default, this will be the gameMode value,
        /// since it then allows for filtering when joining games.
        /// </summary>
        [HideInInspector]
        public string gameServerTitle = string.Empty;

        //all the servers received from the List Server
        private static ServerCollectionJson list;


        void Start()
        {
            connector = GetComponent<ApiConnector>();
            connector.ListServer.ClientApi.onServerListUpdated += UpdateList;

            //continuous game list fetching only for testing
            //connector.ListServer.ClientApi.StartGetServerListRepeat(10);
        }


        private void UpdateList(ServerCollectionJson serverCollection)
        {
            list = serverCollection;
        }


        /// <summary>
        /// Adds a game started by this client to the List Server.
        /// Uses the gameMode as display name. Customize this if you want more filter control.
        /// </summary>
        public static void AddServer()
        {
            Transport transport = Transport.activeTransport;

            string gameMode = PlayerPrefs.GetInt(PrefsKeys.gameMode).ToString();
            Uri uri = transport.ServerUri();
            int port = uri.Port;
            string protocol = uri.Scheme;

            connector.ListServer.ServerApi.AddServer(new ServerJson
            {
                displayName = gameMode,
                protocol = protocol,
                port = port,
                maxPlayerCount = NetworkManager.singleton.maxConnections,
                playerCount = 1
            });
        }


        /// <summary>
        /// Initiates a call to retrieve listed servers from the List Server.
        /// </summary>
        public static void GetServers()
        {
            connector.ListServer.ClientApi.GetServerList();
        }


        /// <summary>
        /// Initiates a call to remove the current open game from the List Server.
        /// </summary>
        public static void RemoveServer()
        {
            if (!connector.ListServer.ServerApi.ServerInList)
                return;

            connector.ListServer.ServerApi.RemoveServer();
        }


        /// <summary>
        /// Updates the player count within a game listed on the List Server,
        /// so that when searching for a game, full games can be excluded.
        /// </summary>
        public static void UpdatePlayerCount(int playerCount)
        {
            if (!connector.ListServer.ServerApi.ServerInList)
                return;

           connector.ListServer.ServerApi.UpdateServer(playerCount);
        }


        /// <summary>
        /// Returns a game from the List Server using some filtering for the current game mode.
        /// Customize this if you want to have more control on matchmaking.
        /// </summary>
        public static string FindGame()
        {
            string gameMode = PlayerPrefs.GetInt(PrefsKeys.gameMode).ToString();

            if (list.servers == null)
                return string.Empty;

            List<ServerJson> servers = list.servers.Where(x => x.displayName == gameMode).ToList();
            servers.RemoveAll(x => x.playerCount == x.maxPlayerCount);

            if (servers.Count > 0)
                return servers[0].address;
            else
                return string.Empty;
        }
    }
}