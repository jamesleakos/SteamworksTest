#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using UnityEngine;
using UnityEditor;
using HeathenEngineering.SteamApi.Editors;

[InitializeOnLoad]
public class SteamworksValidationCheck : AssetPostprocessor
{
    static SteamworksValidationCheck()
    {
        CheckForSteamworksNet();
    }

    static void CheckForSteamworksNet()
    {
        bool SteamworksNetFound = UnityEditor.AssetDatabase.FindAssets("SteamAPICall_t t:MonoScript").Length > 0;
        bool HeathenSteamworksFound = UnityEditor.AssetDatabase.FindAssets("SteamworksFoundationManager t:MonoScript").Length > 0;
        
        if (!SteamworksNetFound || !HeathenSteamworksFound)
        {
            var results = AssetDatabase.FindAssets("t:HeathensSteamworksInstallSettingsComplete");
            if (results.Length > 0)
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<HeathensSteamworksInstallSettingsComplete>(AssetDatabase.GUIDToAssetPath(results[0]));
            }
            else
            {
                results = AssetDatabase.FindAssets("Heathens Steamworks Complete Install Settings");
                if (results.Length > 0)
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<HeathensSteamworksInstallSettingsComplete>(AssetDatabase.GUIDToAssetPath(results[0]));
                }
                else
                {
                    Debug.Log("Additional installation required, please locate the Heathen Steamworks Install Settings to continue!\nThis usually located in the _Heathen Engineering/Steamworks folder.");
                }
            }
        }
    }
}
#endif