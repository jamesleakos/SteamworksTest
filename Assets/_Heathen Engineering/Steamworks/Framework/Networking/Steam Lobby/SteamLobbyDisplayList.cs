#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HeathenEngineering.SteamApi.Networking
{
    public class SteamLobbyDisplayList : MonoBehaviour
    {
        public SteamSettings steamSettings;
        [FormerlySerializedAs("LobbySettings")]
        public SteamworksLobbySettings lobbySettings;
        [FormerlySerializedAs("Filter")]
        public LobbyHunterFilter filter;
        public LobbyRecordBehvaiour recordPrototype;
        public Transform collection;
        [FormerlySerializedAs("OnSearchStarted")]
        public UnityEvent onSearchStarted;
        [FormerlySerializedAs("OnSearchCompleted")]
        public UnityEvent onSearchCompleted;
        [FormerlySerializedAs("OnLobbySelected")]
        public UnitySteamIdEvent onLobbySelected;

        private void OnEnable()
        {
            if (lobbySettings != null && lobbySettings.Manager != null)
            {
                lobbySettings.OnLobbyMatchList.AddListener(HandleBrowseLobbies);
            }
            else
            {
                Debug.LogWarning("SteamLobbyDisplayList requires a HeathenSteamLobbySettings reference which has been registered to a HeathenSteamLobbyManager. If you have provided a HeathenSteamLobbySettings that has been applied to an active HeathenSteamLobbyManager then check to insure that the HeathenSteamLobbyManager has initalized before this control.");
                this.enabled = false;
            }
        }

        private void OnDisable()
        {
            if (lobbySettings != null && lobbySettings.Manager != null)
            {
                lobbySettings.OnLobbyMatchList.RemoveListener(HandleBrowseLobbies);
            }
        }

        private void HandleBrowseLobbies(SteamLobbyLobbyList lobbies)
        {
            foreach (var l in lobbies)
            {
                var go = Instantiate(recordPrototype.gameObject, collection);
                var rec = go.GetComponent<LobbyRecordBehvaiour>();
                rec.SetLobby(l, lobbySettings);
                rec.OnSelected.AddListener(HandleOnSelected);
            }
            onSearchCompleted.Invoke();
        }

        private void HandleOnSelected(CSteamID lobbyId)
        {
            //Pass the event on
            onLobbySelected.Invoke(lobbyId);
        }

        public void BrowseLobbies()
        {
            onSearchStarted.Invoke();
            lobbySettings.Manager.FindMatch(filter);
        }

        public void ClearLobbies()
        {
            while (collection.childCount > 0)
            {
                var target = collection.GetChild(0);
                var go = target.gameObject;
                go.SetActive(false);
                target.parent = null;
                Debug.Log("No Unity in this case we want to set parent directly");
                Destroy(go);
            }
        }
    }
}
#endif