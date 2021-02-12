#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS && MIRROR
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeathenEngineering.SteamApi.Networking.Demo
{
    /// <summary>
    /// <para>An example implamentation of the <see cref="IHeathenGameServerDisplayBrowserEntry"/></para>
    /// <para>This is used to display a Game Server in the server browser.</para>
    /// </summary>
    public class ExampleDisplayGSBrowserEntry : MonoBehaviour, IHeathenGameServerDisplayBrowserEntry, IPointerClickHandler
    {
        public ExampleGSBrowserControl browserParent;
        public GameObject VacIcon;
        public UnityEngine.UI.Text ServerName;
        public UnityEngine.UI.Text PlayerCount;
        public UnityEngine.UI.Text Ping;

        public HeathenGameServerBrowserEntery entry;

        public void OnPointerClick(PointerEventData eventData)
        {
            browserParent.selectedEntry = entry;
            browserParent.DialogRoot.SetActive(true);
            browserParent.DialogQuestionText.text = "Connect to the '" + entry.serverName + "' server?";
        }

        public void SetEntryRecord(HeathenGameServerBrowserEntery entry)
        {
            this.entry = entry;
            this.entry.DataUpdated = new UnityEngine.Events.UnityEvent();
            this.entry.DataUpdated.AddListener(RefreshDisplay);
            RefreshDisplay();
            
        }

        private void RefreshDisplay()
        {
            VacIcon.SetActive(entry.isVAC);
            ServerName.text = entry.serverName;
            PlayerCount.text = entry.currentPlayerCount.ToString() + " / " + entry.maxPlayerCount.ToString();
            Ping.text = entry.ping.ToString();
        }
    }
}
#endif