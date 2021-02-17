/*  This file is part of the "Errantastra" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using Mirror;
using Mirror.Discovery;

namespace Errantastra
{
    /// <summary>
    /// Custom implementation of the Unity Networking NetworkDiscovery class.
    /// This script auto-joins matches found in the local area network on discovery.
    /// </summary>
	public class NetworkDiscoveryCustom : NetworkDiscovery
    {
        public void OnDiscoveredServer(ServerResponse info)
        {               
            NetworkManager.singleton.StartClient(info.uri);

            CancelInvoke();
        }
    }
}
