#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{
    [CustomEditor(typeof(HeathensSteamworksInstallSettingsComplete))]
    public class HeathensSteamworksInstallSettingsCompleteEditor : Editor
    {
        private HeathensSteamworksInstallSettingsComplete pSettings;

        public override void OnInspectorGUI()
        {
            pSettings = target as HeathensSteamworksInstallSettingsComplete;
            pSettings.UpdateInstallStates();

            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Thank you for using Heathen's Steamworks\n\nUse the controlls provided here to install dependencies and componenets", MessageType.Info);
            EditorGUILayout.Space();

            if (pSettings.SteamworksInstall == null)
            {
                EditorGUILayout.HelpBox("You must reference the Steamworks.NET Unity Package before this tool can be used.", MessageType.Error);
            }
            else
            {
                if (pSettings.SteamworksNetFound)
                {
                    EditorGUILayout.HelpBox("Steamworks.NET is installed and ready to use!", MessageType.None);

                    if (pSettings.HeathenSteamworksComplete == null)
                    {
                        EditorGUILayout.HelpBox("You must reference the HeathenSystemsSteamworksComplete_Install Unity Package before this tool can be used to install it.", MessageType.Error);
                    }
                    else
                    {
                        if (pSettings.HeathenSteamworksFound)
                        {
                            EditorGUILayout.Space();
                            EditorGUILayout.HelpBox("Heathen Steamworks is installed and ready to use!", MessageType.None);
                            Rect hRect = EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("");
                            Rect bRect = new Rect(hRect);
                            if (GUI.Button(bRect, "Re-Install Heathen Steamworks", EditorStyles.toolbarButton))
                            {
                                pSettings.InstallHeathenSteamworksPackage();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.Space();
                            EditorGUILayout.HelpBox("Heathen Steamworks needs to be installed", MessageType.Warning);
                            Rect hRect = EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("");
                            Rect bRect = new Rect(hRect);
                            if (GUI.Button(bRect, "Install Heathen Steamworks", EditorStyles.toolbarButton))
                            {
                                pSettings.InstallHeathenSteamworksPackage();
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Steamworks.NET must be installed before other componenets can be used.\n\nYou can get Steamworks.NET for free at https://github.com/rlabrecque/Steamworks.NET/releases \n\nA copy of the Steamworks.NET release that Heathen's Steamworks was bult on is available in the 3rd Parties folder. Heathen's Steamworks should be compatable with any version at or above this but is included to insure access to the tested version.\n\nYou can install from the included Steamworks.NET package or download it from the offical GitHub.", MessageType.Warning);
                    Rect hRect = EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("");

                    Rect bRect = new Rect(hRect);
                    bRect.width = hRect.width / 2f;
                    if (GUI.Button(bRect, "Install Included", EditorStyles.toolbarButton))
                    {
                        pSettings.InstallSteamworksPackage();
                    }
                    bRect.x += bRect.width;
                    if (GUI.Button(bRect, "Download from GIT Hub", EditorStyles.toolbarButton))
                    {
                        Application.OpenURL("https://github.com/rlabrecque/Steamworks.NET/releases");
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
}
#endif