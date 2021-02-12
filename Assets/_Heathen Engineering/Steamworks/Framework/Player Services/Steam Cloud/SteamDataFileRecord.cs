#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeathenEngineering.SteamApi.PlayerServices.UI
{
    /// <summary>
    /// The base class of a UI element to display a <see cref="SteamDataFile"/> object in a <see cref="SteamDataFileList"/>
    /// </summary>
    public class SteamDataFileRecord : Button
    {
        /// <summary>
        /// The text field used to display the <see cref="SteamDataFile"/> name.
        /// </summary>
        [Header("Display Data")]
        public Text FileName;
        /// <summary>
        /// The text field used to display the <see cref="SteamDataFile"/> time stamp.
        /// </summary>
        public Text Timestamp;
        /// <summary>
        /// A UI object that is enabled when this record is selected and disabled when it is not.
        /// </summary>
        public GameObject SelectedIndicator;
        /// <summary>
        /// The address of this specific file on the Steam Remote Storage system.
        /// </summary>
        public SteamworksRemoteStorageManager.FileAddress Address;
        /// <summary>
        /// A pointer back to the parent <see cref="SteamDataFileList"/>
        /// </summary>
        [HideInInspector]
        public SteamDataFileList parentList;

        protected override void Start()
        {
            onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            if (parentList != null)
                parentList.SelectedFile = Address;
        }

        private void Update()
        {
            if(parentList != null && parentList.SelectedFile.HasValue && parentList.SelectedFile.Value.fileName == Address.fileName)
            {
                if (!SelectedIndicator.activeSelf)
                    SelectedIndicator.SetActive(true);
            }
            else
            {
                if (SelectedIndicator.activeSelf)
                    SelectedIndicator.SetActive(false);
            }
        }
    }
}
#endif