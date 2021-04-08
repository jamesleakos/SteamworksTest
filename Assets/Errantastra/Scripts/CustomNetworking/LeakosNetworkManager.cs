using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Errantastra
{
    [AddComponentMenu("")]
    public class LeakosNetworkManager : NetworkManager
    {
        public override void OnServerAddPlayer(NetworkConnection conn)
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
    }
}

