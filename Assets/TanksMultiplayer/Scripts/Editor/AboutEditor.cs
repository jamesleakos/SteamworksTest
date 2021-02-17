/*  This file is part of the "Errantastra" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Errantastra
{
    //our about/help/support editor window
    public class AboutEditor : EditorWindow
    {
        [MenuItem("Window/Errantastra/About")]
        static void Init()
        {
            AboutEditor aboutWindow = (AboutEditor)EditorWindow.GetWindowWithRect
                    (typeof(AboutEditor), new Rect(0, 0, 300, 320), false, "About");
            aboutWindow.Show();
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(70);
            GUILayout.Label("Errantastra", EditorStyles.boldLabel);
            GUILayout.EndHorizontal();
    
            GUILayout.BeginHorizontal();
            GUILayout.Space(70);
            GUILayout.Label("by Rebound Games");
            GUILayout.EndHorizontal();        
            GUILayout.Space(20);

            GUILayout.Label("Info", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Homepage");
            if (GUILayout.Button("Visit", GUILayout.Width(100)))
            {
                Help.BrowseURL("https://www.rebound-games.com");
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("YouTube");
            if (GUILayout.Button("Visit", GUILayout.Width(100)))
            {
                Help.BrowseURL("https://www.youtube.com/user/ReboundGamesTV");
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Twitter");
            if (GUILayout.Button("Visit", GUILayout.Width(100)))
            {
                Help.BrowseURL("https://twitter.com/Rebound_G");
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

        
            GUILayout.Label("Support", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Script Reference");
            if (GUILayout.Button("Visit", GUILayout.Width(100)))
            {
                Help.BrowseURL("https://www.rebound-games.com/docs/tanksmp/");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Community Discord");
            if (GUILayout.Button("Visit", GUILayout.Width(100)))
            {
                Help.BrowseURL("https://discord.gg/8DDMZmS");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Support Forum");
            if (GUILayout.Button("Visit", GUILayout.Width(100)))
            {
                Help.BrowseURL("https://www.rebound-games.com/forum/");
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Unity Forum");
            if (GUILayout.Button("Visit", GUILayout.Width(100)))
            {
                Help.BrowseURL("https://forum.unity3d.com/threads/410465/");
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            
            GUILayout.Label("Support us!", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Review Asset");
            if (GUILayout.Button("Visit", GUILayout.Width(100)))
            {
                Help.BrowseURL("https://assetstore.unity.com/packages/slug/69172?aid=1011lGiF&pubref=editor_tanksmp");
            }
            GUILayout.EndHorizontal();
        }
    }
}