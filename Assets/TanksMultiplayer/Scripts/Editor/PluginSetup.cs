using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace Errantastra
{
    public class PluginSetup : EditorWindow
    {
        private static string packagesPath;
        private static Packages selectedPackage = Packages.UnityNetworking;
        private enum Packages
        {
            UnityNetworking = 0,
            PhotonPUN = 1,
            Mirror
        }


        [MenuItem("Window/Errantastra/Network Setup")]
        static void Init()
        {
            packagesPath = "/Packages/";
            EditorWindow window = EditorWindow.GetWindowWithRect(typeof(PluginSetup), new Rect(0, 0, 360, 260), false, "Network Setup");

            var script = MonoScript.FromScriptableObject(window);
            string thisPath = AssetDatabase.GetAssetPath(script);
            packagesPath = thisPath.Replace("/PluginSetup.cs", packagesPath);
        }


        void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Errantastra - Network Setup", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Please choose the network provider you would like to use:");

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            selectedPackage = (Packages)EditorPrefs.GetInt("TanksMP_Provider", 0);
            selectedPackage = (Packages)EditorGUILayout.EnumPopup(selectedPackage);

            if (EditorPrefs.GetInt("TanksMP_Provider", 0) != (int)selectedPackage)
            {
                EditorPrefs.SetInt("TanksMP_Provider", (int)selectedPackage);
            }

            if (GUILayout.Button("?", GUILayout.Width(20)))
            {
                switch (selectedPackage)
                {
                    case Packages.UnityNetworking:
                        Application.OpenURL("https://unity3d.com/services/multiplayer");
                        break;
                    case Packages.PhotonPUN:
                        Application.OpenURL("https://www.photonengine.com/en/Realtime");
                        break;
                    case Packages.Mirror:
                        Application.OpenURL("https://mirror-networking.com/");
                        break;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button("Step 1: Import Network Package"))
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                AssetDatabase.ImportPackage(packagesPath + selectedPackage.ToString() + ".unitypackage", false);

                //force recompile to let Photon set up platform defines etc.
                string defineGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                if (defineGroup.Contains("TANKSMP")) defineGroup = defineGroup.Replace("TANKSMP", ""); else defineGroup += ";TANKSMP";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineGroup);

                Debug.Log("Errantastra - Network Setup: Wait for the compiler to finish on Step 1, then press Step 2!");
            }

            if (GUILayout.Button("Step 2: Setup Package Contents"))
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                {
                    Debug.LogError("Errantastra - Network Setup: Please wait for the compiler to finish before executing Step 2.");
                    return;
                }

                Setup();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Note:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("For a detailed comparison about features and pricing, please");
            EditorGUILayout.LabelField("refer to the official pages for UNET or Photon. The features");
            EditorGUILayout.LabelField("of this asset are the same across both multiplayer services.");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Please read the PDF documentation for further details.");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Support links: Window > Errantastra > About.");
        }


        void Setup()
        {
            string[] scenes = System.IO.Directory.GetFiles(".", "*.unity", System.IO.SearchOption.AllDirectories);

            if (selectedPackage == Packages.UnityNetworking)
            {
                NetworkManagerCustom netManager = (NetworkManagerCustom)AssetDatabase.LoadAssetAtPath("Assets/Errantastra/Prefabs/Network.prefab", typeof(NetworkManagerCustom));
                System.Reflection.PropertyInfo playerPrefab = netManager.GetType().GetProperty("playerPrefab");
                playerPrefab.SetValue(netManager, Resources.Load("TankFree"), null);
                AssetDatabase.ImportAsset("Assets/Errantastra/Scripts/GameManager.cs");

                Debug.Log("Errantastra - Setup Done!");
            }

            if (selectedPackage == Packages.Mirror)
            {
                NetworkManagerCustom netManager = (NetworkManagerCustom)AssetDatabase.LoadAssetAtPath("Assets/Errantastra/Prefabs/Network.prefab", typeof(NetworkManagerCustom));
                System.Reflection.FieldInfo playerPrefab = netManager.GetType().GetField("playerPrefab");
                playerPrefab.SetValue(netManager, Resources.Load("TankFree"));
                AssetDatabase.ImportAsset("Assets/Errantastra/Scripts/GameManager.cs");
            }

            if (selectedPackage == Packages.Mirror && EditorUtility.DisplayDialog("Step 2: Setup Package Contents for Mirror", "Step 2 is a manual step.\n\n" +
                                "Please open all game scenes and save them once! This is needed so that Unity correctly assigns ViewIDs to networked objects.", "I understand!"))
            { }

            if (selectedPackage == Packages.PhotonPUN)
            {
                #if !PUN_2_OR_NEWER
                    Debug.LogError("Errantastra - Network Setup: Could not find Photon Scripting Define. Did you import Photon yet?");
                #else

                if(EditorUtility.DisplayDialog("Step 2: Setup Package Contents for Photon", "Step 2 is a manual step.\n\n" +
                                               "Please open all game scenes, unpack the 'ObjectSpawner' prefabs and save the scene. This is needed so that Photon correctly assigns ViewIDs to networked objects.\n\n" +
                                               "For a screenshot on what to do, please see our documentation PDF.", "I understand!"))
                {}
                #endif
            }

            foreach (string scene in scenes)
            {
                if (scene.EndsWith("Intro.unity"))
                {
                    EditorSceneManager.OpenScene(scene);
                    break;
                }
            }
        }
    }
}