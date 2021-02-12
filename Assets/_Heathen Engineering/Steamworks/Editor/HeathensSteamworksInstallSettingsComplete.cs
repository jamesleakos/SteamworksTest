#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{

    [CreateAssetMenu(menuName = "Steamworks/Complete Installer Settings", order = 9999)]
    public class HeathensSteamworksInstallSettingsComplete : ScriptableObject
    {
#if UNITY_EDITOR
        public Object SteamworksInstall;
        public Object HeathenSteamworksComplete; 
        [HideInInspector]
        public bool SteamworksNetFound = false;
        [HideInInspector]
        public bool HeathenSteamworksFound = false;
#endif
        public void UpdateInstallStates()
        {
#if UNITY_EDITOR
            SteamworksNetFound = UnityEditor.AssetDatabase.FindAssets("SteamAPICall_t t:MonoScript").Length > 0;
            HeathenSteamworksFound = UnityEditor.AssetDatabase.FindAssets("SteamworksFoundationManager t:MonoScript").Length > 0;
#else
            Debug.LogWarning("This (HeathensSteamworksInstallSettings.UpdateInstallStates) can only be called from the editor");
#endif
        }

        public void InstallSteamworksPackage()
        {
#if UNITY_EDITOR
            if (SteamworksInstall != null)
            {
                Debug.Log("InstallSteamworksPackage()");
                string steamworksPackage = UnityEditor.AssetDatabase.GetAssetPath(SteamworksInstall);
                UnityEditor.AssetDatabase.ImportPackage(steamworksPackage, true);
            }
            else
            {
                Debug.LogError("You must indicate the Steamworks.NET package to install");
            }
#else
            Debug.LogWarning("This (HeathensSteamworksInstallSettings.InstallSteamworksPackage) can only be called from the editor");
#endif
        }

        public void InstallHeathenSteamworksPackage()
        {
#if UNITY_EDITOR
            if (SteamworksInstall != null)
            {
                Debug.Log("InstallHeathenSteamworksPackage()");
                string steamworksPackage = UnityEditor.AssetDatabase.GetAssetPath(HeathenSteamworksComplete);
                UnityEditor.AssetDatabase.ImportPackage(steamworksPackage, true);
            }
            else
            {
                Debug.LogError("You must indicate the Heathen Steamworks package to install");
            }
#else
            Debug.LogWarning("This (HeathensSteamworksInstallSettings.InstallHeathenSteamworksPackage) can only be called from the editor");
#endif
        }
    }
}
#endif