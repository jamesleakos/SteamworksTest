/*  This file is part of the "Tanks Multiplayer" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Mirror;
using HeathenEngineering.SteamApi.Networking;
using HeathenEngineering.SteamApi.Foundation;
using UnityEngine.Serialization;
using UnityEngine.Events;



namespace Errantastra
{
    /// <summary>
    /// Custom implementation of the Unity Networking NetworkManager class. This script is
    /// responsible for connecting to the Matchmaker, spawning players and handling disconnects.
    /// </summary>
	public class NetworkManagerCustom : NetworkManager
    {
        // from Heathen Custom Network Manager

        public UnityEvent OnHostStarted;
        public UnityEvent OnServerStarted;
        public UnityEvent OnClientStarted;
        public UnityEvent OnServerStopped;
        public UnityEvent OnClientStopped;
        public UnityEvent OnHostStopped;
        [Obsolete("No longer used.")]
        public UnityEvent OnRegisterServerMessages;
        [Obsolete("No longer used.")]
        public UnityEvent OnRegisterClientMessages;

        // End


        private NetworkListServer listServer;

        [FormerlySerializedAs("SteamSettings")]
        public SteamSettings steamSettings;


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
            OnServerStarted.Invoke();
            base.OnStartServer();

            NetworkServer.RegisterHandler<JoinMessage>(OnServerAddPlayer);
        }

        public override void OnStartHost()
        { OnHostStarted.Invoke(); }

        public override void OnStartClient()
        {
            OnClientStarted.Invoke();
            networkSceneName = "";
        }
        public override void OnStopClient()
        { OnClientStopped.Invoke(); }
        public override void OnStopHost()
        { OnHostStopped.Invoke(); }

        /// <summary>
        /// Starts initializing and connecting to a game. Depends on the selected network mode.
        /// </summary>
        public static IEnumerator StartMatch()
        {
            Debug.Log("StartMatch Start");

            //add a filter attribute considering the selected game mode on the matchmaker as well
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

            Debug.Log("StartMatch End");

        }

        /// <summary>
        /// Override for the callback received when the list of matchmaker matches returns.
        /// This method decides if we can join a game or need to create our own session.
        /// </summary>
        public override void OnClientDisconnect(NetworkConnection conn)
        {
            Debug.Log("OnClientDisconnect");

            //do not switch scenes automatically when the game over screen is being shown already
            if (GameManager.GetInstance() != null && GameManager.GetInstance().ui.gameOverMenu.activeInHierarchy)
                return;

            if(!conn.isAuthenticated)
                Debug.Log("Timeout: Mirror did not find any matches on the Master Client we are connecting to. Eventually creating our own room...");

            //switch from the online to the offline scene after connection is closed
            if (!NetworkManager.IsSceneActive(SceneManager.GetSceneByBuildIndex(0).name))
            {
                Debug.Log("OnClientDIsconnect Here 1");
                StopHost();
                SceneManager.LoadScene(0);
            }
            else
            {
                Debug.Log("OnClientDIsconnect Here 1");
                CreateMatch();

            }

            Debug.Log("OnClientDisconnect End");

        }

        //creates a new match with default values
        void CreateMatch()
        {
            Debug.Log("CreateMatch Start");
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
            Debug.Log("We have just one scene for now, but here is where we would change that. This would overwrite the dfault online scene assigned in the editor");
            //onlineScene = matchingScenes[UnityEngine.Random.Range(0, matchingScenes.Count)];

            //double check to only start matchmaking match in online mode
            if (PlayerPrefs.GetInt(PrefsKeys.networkMode) == 0 && (singleton as NetworkManagerCustom).listServer != null)
            {
                (singleton as NetworkManagerCustom).listServer.gameServerTitle = gameMode.ToString();
            }

            //start hosting the match
            StartHost();

            Debug.Log("CreateMatch End");

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

            Debug.Log("NetworkManagerCustom.OnClientConnect End");

        }

        /// <summary>
        /// Override for the callback received on the server when a client requests creating its player prefab.
        /// Nearly the same as in the UNET source OnServerAddPlayerInternal method, but reading out the message passed in,
        /// effectively handling user player prefab selection, assignment to a team and spawning it at the team area.
        /// </summary>
	    void OnServerAddPlayer(NetworkConnection conn, JoinMessage message)
        {
            Debug.Log("NetworkManagerCustom.OnServerAddPlayer");
            //read the user message
            if (string.IsNullOrEmpty(message.playerName))
            {
                if (LogFilter.Debug) Debug.Log("OnServerAddPlayer called with empty player name!");
                //return;
            }
      
            //read prefab index to spawn out of the JoinMessage the client sent along with its request
            //then try to get prefab of the registered spawnable prefabs in the NetworkManager inspector (Spawn Info section)
	        GameObject playerObj = null;
            Debug.Log("Here is where we could choose between different player prefabs");
            //playerObj = spawnPrefabs[0];
            playerObj = playerPrefab;

            //get the team value for this player
            int teamIndex = GameManager.GetInstance().GetTeamIndex();
            //get spawn position for this team and instantiate the player there
            Vector3 startPos = GameManager.GetInstance().GetSpawnPosition();
	        playerObj = (GameObject)Instantiate(playerObj, startPos, Quaternion.identity);
            
            //assign name (also in JoinMessage) and team to Player component
            Player p = playerObj.GetComponent<Player>();
            p.AddName(message.playerName);
            p.teamIndex = teamIndex;
            Debug.Log(p.myName + " is on Team " + p.teamIndex);

            //update the game UI to correctly display the increased team size
            //GameManager.GetInstance().size[p.teamIndex]++;
            //GameManager.GetInstance().ui.OnTeamSizeChanged(p.teamIndex, 0, 0);

            //finally map the player gameobject to the connection requesting it
	        NetworkServer.AddPlayerForConnection(conn, playerObj);

            if(listServer != null)
            {
                NetworkListServer.UpdatePlayerCount(NetworkServer.connections.Count);
            }

            Debug.Log("NetworkManagerCustom.OnServerAddPlayer End");

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
            if (GameManager.GetInstance().size.Count >= p.teamIndex)
            {
                GameManager.GetInstance().size[p.teamIndex]--;
                GameManager.GetInstance().ui.OnTeamSizeChanged(p.teamIndex, 0, 0);
            }

            base.OnServerDisconnect(conn);
            if(listServer != null)
            {
                NetworkListServer.UpdatePlayerCount(NetworkServer.connections.Count);
            }

            Debug.Log("NetworkManagerCustom.OnServerDisconnect End");

        }


        /// <summary>
        /// Override for the callback received when the server shut down.
        /// </summary>
        public override void OnStopServer()
        {
            Debug.Log("NetworkManagerCustom.OnStopServer");
            OnServerStopped.Invoke();
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
            try
            {
                message.playerName = steamSettings.client.user.DisplayName;
            } catch
            {
                message.playerName = "Player" + UnityEngine.Random.Range(0, 10).ToString();
            }
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
