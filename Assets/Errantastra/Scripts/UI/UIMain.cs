/*  This file is part of the "Tanks Multiplayer" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Errantastra
{
    /// <summary>
    /// UI script for all elements, settings and user interactions in the menu scene.
    /// </summary>
    public class UIMain : MonoBehaviour
    {

        //initialize player selection in Settings window
        //if this is the first time launching the game, set initial values
        void Start()
        {
            //set initial values for all settings         
            if (!PlayerPrefs.HasKey(PrefsKeys.gameMode)) PlayerPrefs.SetInt(PrefsKeys.gameMode, 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Tries to enter the game scene. Sets the loading screen active while connecting to the
        /// Matchmaker and starts the timeout coroutine at the same time.
        /// </summary>
        public void Play()
        {
            StartCoroutine(NetworkManagerCustom.StartMatch());
            StartCoroutine(HandleTimeout());
        }


        //coroutine that waits x seconds before cancelling joining a match
        IEnumerator HandleTimeout()
        {
            //timeout by transport layer usually happens after 10 seconds (change in Mirror transport class)
            //in case we are still not connected after the timeout happened, display the error window
            yield return new WaitForSeconds(11);

            if (Mirror.NetworkClient.active)
                yield break;

            //timeout has passed, we would like to stop joining a game now
            NetworkManagerCustom.singleton.StopHost();

            //display connection issue window
            OnConnectionError();
        }


        //activates the connection error window to be visible
        void OnConnectionError()
        {
            StopCoroutine(HandleTimeout());
        }
    }
}
