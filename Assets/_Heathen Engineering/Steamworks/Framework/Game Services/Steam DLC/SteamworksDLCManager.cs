#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.SteamApi.GameServices
{
    /// <summary>
    /// Links and manages <see cref="SteamDLCData"/> objects. 
    /// This behaviour handles updating the status of each <see cref="SteamDLCData"/> object.
    /// </summary>
    /// <remarks>Steam DLC or Downloadable Content is defined on the Steam API in your Steam Portal.
    /// Please carfully read <a href="https://partner.steamgames.com/doc/store/application/dlc">https://partner.steamgames.com/doc/store/application/dlc</a> before designing features are this concept.</remarks>
    public class SteamworksDLCManager : MonoBehaviour
    {
        /// <summary>
        /// The collection of <see cref="SteamDLCData"/> objects to update on start and listen for installation callbacks for.
        /// </summary>
        public List<SteamDLCData> DLC = new List<SteamDLCData>();

        #region Callbacks
        private Callback<DlcInstalled_t> m_DlcInstalled;
        #endregion

        private void Start()
        {
            m_DlcInstalled = Callback<DlcInstalled_t>.Create(HandleDlcInstalled);

            UpdateAll();
        }

        private void HandleDlcInstalled(DlcInstalled_t param)
        {
            var target = DLC.FirstOrDefault(p => p.AppId == param.m_nAppID);
            if (target != null)
            {
                target.UpdateStatus();
            }
        }

        /// <summary>
        /// Requests the status data of all DLC be updated.
        /// </summary>
        public void UpdateAll()
        {
            foreach (var dlc in DLC)
            {
                dlc.UpdateStatus();
            }
        }

        /// <summary>
        /// Gets the <see cref="SteamDLCData"/> object with the matching <see cref="AppId_t"/>
        /// </summary>
        /// <param name="AppId">The ID to match</param>
        /// <returns></returns>
        public SteamDLCData GetDLC(AppId_t AppId)
        {
            return DLC.FirstOrDefault(p => p.AppId == AppId);
        }

        /// <summary>
        /// Gets the <see cref="SteamDLCData"/> object with the matching name
        /// </summary>
        /// <param name="name">The name to match</param>
        /// <returns></returns>
        public SteamDLCData GetDLC(string name)
        {
            return DLC.FirstOrDefault(p => p.name == name);
        }
    }
}
#endif