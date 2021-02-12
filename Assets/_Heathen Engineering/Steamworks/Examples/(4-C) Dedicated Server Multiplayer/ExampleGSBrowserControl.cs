#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS && MIRROR
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Networking.Demo
{
    /// <summary>
    /// Control script for the Game Server demo scene to control the funcitonality of the server browser
    /// </summary>
    public class ExampleGSBrowserControl : MonoBehaviour
    {
        public HeathenGameServerBrowser browser;
        public GameObject RecordTemplate;
        public GameObject DialogRoot;
        public UnityEngine.UI.Text DialogQuestionText;
        public Transform InternetRoot;
        public Transform FavoriteRoot;
        public Transform FriendRoot;
        public Transform LANRoot;
        public Transform HistoryRoot;
        public Transform SpectatorRoot;

        public HeathenGameServerBrowserEntery selectedEntry;

        private void OnEnable()
        {
            browser.InternetServerListUpdated.AddListener(OnInternetServerUpdate);
            browser.FavoriteServerListUpdated.AddListener(OnFavoriteServerUpdate);
            browser.FriendsServerListUpdated.AddListener(OnFriendServerUpdate);
            browser.LANServerListUpdated.AddListener(OnLANServerUpdate);
            browser.HistoryServerListUpdated.AddListener(OnHistoryServerUpdate);
            browser.SpectatorServerListUpdated.AddListener(OnSpectatorServerUpdate);
            SetDefaultFilter();
        }

        public void ConnectToSelected()
        {
            selectedEntry.SwitchServer();
        }

        /// <summary>
        /// SteamMatchmakingServer Filters can be very complex or very simple
        /// This example works with the Default behaviour of Heathen Game Server Manager to filter on games with a matching AppId set in the Game Data field of the server
        /// see <a href="https://partner.steamgames.com/doc/api/ISteamMatchmakingServers#MatchMakingKeyValuePair_t">https://partner.steamgames.com/doc/api/ISteamMatchmakingServers#MatchMakingKeyValuePair_t</a> for more information.
        /// As a result of this defualt filter your general searches using the demo app ID are likely to comeback with 0 results or 1 if you are running a server build now
        /// To see an unfiltered set of results (can be thousands) remove this filter by commenting it out.
        /// </summary>
        private void SetDefaultFilter()
        {
            browser.filter.Clear();
            browser.filter.Add(new Steamworks.MatchMakingKeyValuePair_t() { m_szKey = "gamedataand", m_szValue = "AppId=" + browser.steamSettings.applicationId.m_AppId.ToString() });
        }

        private void ClearChildren(Transform root)
        {
            //Clear the children
            var children = new List<GameObject>();
            foreach (Transform t in root)
                children.Add(t.gameObject);

            while(children.Count > 0)
            {
                var target = children[0];
                children.Remove(target);
                Destroy(target);
            }
        }

        private void PopulateChildren(Transform root, List<HeathenGameServerBrowserEntery> list)
        {
            foreach (var entry in list)
            {
                var go = Instantiate(RecordTemplate, root);
                var iFace = go.GetComponent<IHeathenGameServerDisplayBrowserEntry>();
                iFace.SetEntryRecord(entry);
            }
        }

        private void OnSpectatorServerUpdate()
        {
            ClearChildren(SpectatorRoot);
            PopulateChildren(SpectatorRoot, browser.SpectatorServers);
        }

        private void OnHistoryServerUpdate()
        {
            ClearChildren(HistoryRoot);
            PopulateChildren(HistoryRoot, browser.HistoryServers);
        }

        private void OnLANServerUpdate()
        {
            ClearChildren(LANRoot);
            PopulateChildren(LANRoot, browser.LANServers);
        }

        private void OnFriendServerUpdate()
        {
            ClearChildren(FriendRoot);
            PopulateChildren(FriendRoot, browser.FriendsServers);
        }

        private void OnFavoriteServerUpdate()
        {
            ClearChildren(FavoriteRoot);
            PopulateChildren(FavoriteRoot, browser.FavoritesServers);
        }

        private void OnInternetServerUpdate()
        {
            ClearChildren(InternetRoot);
            PopulateChildren(InternetRoot, browser.InternetServers);
        }
    }
}
#endif