/*  This file is part of the "Errantastra" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using HeathenEngineering.SteamApi.Networking;


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
            base.Start();

            listServer = GetComponent<NetworkListServer>();
        }

        /// <summary>
        /// Adding custom handlers invoked when they receive messages on the server.
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();

            //NetworkServer.RegisterHandler<JoinMessage>(OnServerAddPlayer);
        }


        /// <summary>
        /// Starts initializing and connecting to a game. Depends on the selected network mode.
        /// </summary>
        public static IEnumerator StartMatch(NetworkMode mode)
        {
            switch (mode)
            {
                //tries to retrieve a list of all games currently available on the matchmaker
                case NetworkMode.Online:

                    //add a filter attribute considering the selected game mode on the matchmaker as well
                    if ((singleton as NetworkManagerCustom).listServer != null)
                    {
                        NetworkListServer.GetServers();
                        //give the request some time to fetch server lists
                        yield return new WaitForSeconds(3);

                        string serverAddress = NetworkListServer.FindGame();
                        if (!string.IsNullOrEmpty(serverAddress))
                        {
                            singleton.networkAddress = serverAddress;
                        }
                        else
                        {
                            //add own client as server
                            (singleton as NetworkManagerCustom).CreateMatch();
                            NetworkListServer.AddServer();
                            yield break;
                        }
                    }

                    singleton.StartClient();
                    break;

                //search for open LAN games on the current network, otherwise open a new one
                case NetworkMode.LAN:
                    singleton.StartCoroutine((singleton as NetworkManagerCustom).DiscoverNetwork());
                    break;

                //start a single LAN game but do not make it public over the network (offline)
                case NetworkMode.Offline:
                    (singleton as NetworkManagerCustom).CreateMatch();
                    break;
            }
        }


        /// <summary>
        /// Override for the callback received when the list of matchmaker matches returns.
        /// This method decides if we can join a game or need to create our own session.
        /// </summary>
        public override void OnClientDisconnect(NetworkConnection conn)
        {
            //do not switch scenes automatically when the game over screen is being shown already
            if (GameManager.GetInstance() != null && GameManager.GetInstance().ui.gameOverMenu.activeInHierarchy)
                return;

            Debug.Log("Mirror did not find any matches on the Master Client we are connecting to. Creating our own room...");

            //switch from the online to the offline scene after connection is closed
            if (!NetworkManager.IsSceneActive(offlineScene))
                SceneManager.LoadScene(offlineScene);
            else
                CreateMatch();
        }


        /// <summary>
        /// Searches for other LAN games in the current network.
        /// </summary>
        public IEnumerator DiscoverNetwork()
        {
            //start listening to other hosts
            NetworkDiscoveryCustom discovery = GetComponent<NetworkDiscoveryCustom>();
            discovery.StartDiscovery();

            //wait few seconds for broadcasts to arrive
            yield return new WaitForSeconds(8);

            //we haven't found a match, open our own
            if (!NetworkClient.isConnected)
            {
                CreateMatch();
                discovery.AdvertiseServer();
            }
        }


        //creates a new match with default values
        void CreateMatch()
        {
            int gameMode = PlayerPrefs.GetInt(PrefsKeys.gameMode);
            //load the online scene randomly out of all available scenes for the selected game mode
            //we are checking for a naming convention here, if a scene starts with the game mode abbreviation
            string activeGameMode = ((GameMode)PlayerPrefs.GetInt(PrefsKeys.gameMode)).ToString();
            List<string> matchingScenes = new List<string>();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string[] scenePath = SceneUtility.GetScenePathByBuildIndex(i).Split('/');
                if (scenePath[scenePath.Length - 1].StartsWith(activeGameMode))
                {
                    matchingScenes.Add(scenePath[scenePath.Length - 1].Replace(".unity", ""));
                }
            }

            //check that your scene begins with the game mode abbreviation
            if (matchingScenes.Count == 0)
            {
                Debug.LogWarning("No Scene for selected Game Mode found in Build Settings!");
                return;
            }

            //get random scene out of available scenes and assign it as the online scene
            onlineScene = matchingScenes[UnityEngine.Random.Range(0, matchingScenes.Count)];

            //double check to only start matchmaking match in online mode
            if (PlayerPrefs.GetInt(PrefsKeys.networkMode) == 0 && (singleton as NetworkManagerCustom).listServer != null)
            {
                (singleton as NetworkManagerCustom).listServer.gameServerTitle = gameMode.ToString();
            }

            //start hosting the match
            StartHost();
        }


        /// <summary>
        /// Override for callback received (on the client) when joining a game.
        /// Same as in the UNET source, but modified AddPlayer method with more parameters.
        /// </summary>
        public override void OnClientConnect(NetworkConnection conn)
	    {
            //if the client connected but did not load the online scene
            if (!clientLoadedScene)
            {
                //Ready/AddPlayer is usually triggered by a scene load completing (see OnClientSceneChanged).
                //if no scene was loaded, maybe because we don't have separate online/offline scenes, then do it here.
                ClientScene.Ready(conn);
                if (autoCreatePlayer && ClientScene.localPlayer == null)
	            {
                    //conn.Send(GetJoinMessage());
	            }
	        }
        }


        /// <summary>
        /// Override for the callback received on the server when a client requests creating its player prefab.
        /// Nearly the same as in the UNET source OnServerAddPlayerInternal method, but reading out the message passed in,
        /// effectively handling user player prefab selection, assignment to a team and spawning it at the team area.
        /// </summary>
	    void OnServerAddPlayer(NetworkConnection conn, JoinMessage message)
	    {
            //read the user message
            if(message == null)
            {
                if (LogFilter.Debug) Debug.Log("OnServerAddPlayer called without JoinMessage!");
                return;
            }
      
            //read prefab index to spawn out of the JoinMessage the client sent along with its request
            //then try to get prefab of the registered spawnable prefabs in the NetworkManager inspector (Spawn Info section)
	        GameObject playerObj = null;
            if(spawnPrefabs.Count > message.prefabId)
            {
                playerObj = spawnPrefabs[message.prefabId];
            }

            //get the team value for this player
            int teamIndex = GameManager.GetInstance().GetTeamFill();
            //get spawn position for this team and instantiate the player there
            Vector3 startPos = GameManager.GetInstance().GetSpawnPosition(teamIndex);
	        playerObj = (GameObject)Instantiate(playerObj, startPos, Quaternion.identity);
            
            //assign name (also in JoinMessage) and team to Player component
            HumanPlayer p = playerObj.GetComponent<HumanPlayer>();
            p.myName = message.playerName;
            p.teamIndex = teamIndex;
            
            //update the game UI to correctly display the increased team size
            GameManager.GetInstance().size[p.teamIndex]++;
            GameManager.GetInstance().ui.OnTeamSizeChanged(SyncListInt.Operation.OP_ADD, p.teamIndex, 0, 0);

            //finally map the player gameobject to the connection requesting it
	        NetworkServer.AddPlayerForConnection(conn, playerObj);

            if(listServer != null)
            {
                NetworkListServer.UpdatePlayerCount(NetworkServer.connections.Count);
            }
	    }


        /// <summary>
        /// Override for the callback received on the server when a client disconnects from the game.
        /// Updates the game UI to correctly display the decreased team size.  This is not called for
        /// the server itself, thus the workaround in GameManager's OnHostMigration method is needed.
        /// </summary>
        public override void OnServerDisconnect(NetworkConnection conn)
        {   
            HumanPlayer p = conn.identity.gameObject.GetComponent<HumanPlayer>();

            GameManager.GetInstance().size[p.teamIndex]--;
            GameManager.GetInstance().ui.OnTeamSizeChanged(SyncListInt.Operation.OP_REMOVEAT, p.teamIndex, 0, 0);
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
            if(listServer != null)
            {
                NetworkListServer.RemoveServer();
            }
        }


        //constructs the JoinMessage for the client by reading its device settings
        private JoinMessage GetJoinMessage()
        {
            JoinMessage message = new JoinMessage();
            message.prefabId = short.Parse(Encryptor.Decrypt(PlayerPrefs.GetString(PrefsKeys.activeTank)));
            message.playerName = PlayerPrefs.GetString(PrefsKeys.playerName);
            return message;
        }
	}


    /// <summary>
    /// Network Mode selection for preferred network type.
    /// </summary>
    public enum NetworkMode
    {
        Online,
        LAN,
        Offline
    }
    
    
    /// <summary>
    /// The client message constructed for the add player request to the server.
    /// You can extend this class to send more data at the point of joining a match.
    /// </summary>
    [System.Serializable]
    public class JoinMessage : NetworkMessage
    {
        /// <summary>
        /// The player prefab index to be spawned, selected by the user.
        /// </summary>
        public short prefabId;
        
        /// <summary>
        /// The user name entered in the game settings.
        /// </summary>
        public string playerName;
    }
}
