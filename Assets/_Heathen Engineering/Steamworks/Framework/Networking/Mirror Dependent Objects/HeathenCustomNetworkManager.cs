#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS && MIRROR
using Mirror;
using UnityEngine;
using System;
using UnityEngine.Events;
using Errantastra;

namespace HeathenEngineering.SteamApi.Networking
{
    public class HeathenCustomNetworkManager : Mirror.NetworkManager
    {
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

        public override void OnStartHost()
        { OnHostStarted.Invoke(); }
        public override void OnStartServer()
        { OnServerStarted.Invoke(); }
        
        public override void OnStartClient()
        {
            OnClientStarted.Invoke();
        }
        public override void OnStopServer()
        { OnServerStopped.Invoke(); }
        public override void OnStopClient()
        { OnClientStopped.Invoke(); }
        public override void OnStopHost()
        { OnHostStopped.Invoke(); }

        public override void OnServerAddPlayer (NetworkConnection conn)
        {
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
    }
}
#endif