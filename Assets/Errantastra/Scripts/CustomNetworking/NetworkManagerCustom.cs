/*  This file is part of the "Tanks Multiplayer" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

namespace Errantastra
{
    /// <summary>
    /// Custom implementation of the Unity Networking NetworkManager class. This script is
    /// responsible for connecting to the Matchmaker, spawning players and handling disconnects.
    /// </summary>
	public class NetworkManagerCustom : NetworkManager
    {
        private NetworkListServer listServer;


        public override void Start()
        {
            Debug.Log("NetworkManagerCustom.Start");
            base.Start();

            listServer = GetComponent<NetworkListServer>();
        }


        /// <summary>
        /// Adding custom handlers invoked when they receive messages on the server.
        /// </summary>
        public override void OnStartServer()
        {
            Debug.Log("NetworkManagerCustom.OnStartServer");
            base.OnStartServer();

            NetworkServer.RegisterHandler<JoinMessage>(OnServerAddPlayer);
        }

        /// <summary>
        /// Starts initializing and connecting to a game. Depends on the selected network mode.
        /// </summary>
        public static IEnumerator StartMatch()
        {
            Debug.Log("NetworkManagerCustom.StartMatch");
            //add a filter attribute considering the selected game mode on the matchmaker as well

            Debug.Log("singleton = " + (singleton as NetworkManagerCustom).listServer);

            if ((singleton as NetworkManagerCustom).listServer != null)
            {
                Debug.Log("NetworkManagerCustom.StartMatch ListServer exists");
                NetworkListServer.GetServers();
                //give the request some time to fetch server lists
                yield return new WaitForSeconds(3);

                string serverAddress = NetworkListServer.FindGame();
                if (!string.IsNullOrEmpty(serverAddress))
                {
                    Debug.Log("NetworkManagerCustom.StartMatch Have a Server Addres");
                    singleton.networkAddress = serverAddress;
                }
                else
                {
                    Debug.Log("NetworkManagerCustom.StartMatch No Server Addres");
                    //add own client as server
                    (singleton as NetworkManagerCustom).CreateMatch();
                    NetworkListServer.AddServer();
                    yield break;
                }
            }

            singleton.StartClient();
        }

        /// <summary>
        /// Override for the callback received when the list of matchmaker matches returns.
        /// This method decides if we can join a game or need to create our own session.
        /// </summary>
        public override void OnClientDisconnect(NetworkConnection conn)
        {
            Debug.Log("NetworkManagerCustom.OnClientDisconnect");
            //do not switch scenes automatically when the game over screen is being shown already
            if (GameManager.GetInstance() != null && GameManager.GetInstance().ui.gameOverMenu.activeInHierarchy)
                return;

            if(!conn.isAuthenticated)
                Debug.Log("Timeout: Mirror did not find any matches on the Master Client we are connecting to. Eventually creating our own room...");

            //switch from the online to the offline scene after connection is closed
            if (!NetworkManager.IsSceneActive(SceneManager.GetSceneByBuildIndex(0).name))
            {
                StopHost();
                SceneManager.LoadScene(0);
            }
            else
                CreateMatch();
        }

        //creates a new match with default values
        void CreateMatch()
        {
            //start hosting the match
            StartHost();
        }

        /// <summary>
        /// Override for callback received (on the client) when joining a game.
        /// Same as in the UNET source, but modified AddPlayer method with more parameters.
        /// </summary>
        public override void OnClientConnect(NetworkConnection conn)
        {
            Debug.Log("NetworkManagerCustom.OnClientConnect");
            //if the client connected but did not load the online scene
            if (!clientLoadedScene)
            {
                //Ready/AddPlayer is usually triggered by a scene load completing (see OnClientSceneChanged).
                //if no scene was loaded, maybe because we don't have separate online/offline scenes, then do it here.
                ClientScene.Ready(conn);
                if (autoCreatePlayer && ClientScene.localPlayer == null)
	            {
                    conn.Send(GetJoinMessage());
	            }
	        }
        }

        /// <summary>
        /// Override for the callback received on the server when a client requests creating its player prefab.
        /// Nearly the same as in the UNET source OnServerAddPlayerInternal method, but reading out the message passed in,
        /// effectively handling user player prefab selection, assignment to a team and spawning it at the team area.
        /// </summary>
	    public void OnServerAddPlayer(NetworkConnection conn, JoinMessage message)
        {
            Debug.Log("OnServerAddPlayer");
            Transform startPos = GameManager.GetInstance().GetSpawnPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);
            player.GetComponent<HumanPlayer>().teamIndex = GameManager.GetInstance().CreateTeam();
            Debug.Log("Player added to team " + player.GetComponent<HumanPlayer>().teamIndex);
            // instantiating a "Player" prefab gives it the name "Player(clone)"
            // => appending the connectionId is WAY more useful for debugging!
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);
        }

        /// <summary>
        /// Override for the callback received on the server when a client disconnects from the game.
        /// Updates the game UI to correctly display the decreased team size.  This is not called for
        /// the server itself, thus the workaround in GameManager's OnHostMigration method is needed.
        /// </summary>
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            Debug.Log("NetworkManagerCustom.OnServerDisconnect");
            if (conn == null || conn.identity == null) return;
            Player p = conn.identity.gameObject.GetComponent<Player>();

            //additional check for the count, since disconnecting as a server otherwise throws an error
            //if (GameManager.GetInstance().size.Count >= p.teamIndex)
            //{
            //    GameManager.GetInstance().size[p.teamIndex]--;
            //    GameManager.GetInstance().ui.OnTeamSizeChanged(SyncList<int>.Operation.OP_REMOVEAT, p.teamIndex, 0, 0);
            //}

            base.OnServerDisconnect(conn);
            if(listServer != null)
            {
                NetworkListServer.UpdatePlayerCount(NetworkServer.connections.Count);
            }
        }
        
        
        /// <summary>
        /// Override for the callback received when a client disconnected.
        /// Eventual cleanup of internal high level API UNET variables.
        /// </summary>
        public override void OnStopClient()
        {
            Debug.Log("NetworkManagerCustom.OnStopClient");
            //because we are not using the automatic scene switching and cleanup by Unity Networking,
            //the current network scene is still set to the online scene even after disconnecting.
            //so to clean that up for internal reasons, we simply set it to an empty string here
            networkSceneName = "";
        }


        /// <summary>
        /// Override for the callback received when the server shut down.
        /// </summary>
        public override void OnStopServer()
        {
            Debug.Log("NetworkManagerCustom.OnStopServer");
            NetworkServer.UnregisterHandler<JoinMessage>();

            if (listServer != null)
            {
                NetworkListServer.RemoveServer();
            }
        }


        //constructs the JoinMessage for the client by reading its device settings
        private JoinMessage GetJoinMessage()
        {
            Debug.Log("NetworkManagerCustom.GetJoinMessage");
            JoinMessage message = new JoinMessage();
            message.playerName = "Player" + UnityEngine.Random.Range(0,10000);
            return message;
        }
	}
    
    /// <summary>
    /// The client message constructed for the add player request to the server.
    /// You can extend this class to send more data at the point of joining a match.
    /// </summary>
    [System.Serializable]
    public struct JoinMessage : NetworkMessage
    {
        /// <summary>
        /// The user name entered in the game settings.
        /// </summary>
        public string playerName;
    }
}
