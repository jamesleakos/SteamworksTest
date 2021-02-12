#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using HeathenEngineering.Scriptable;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Demo
{
    public class ConcatinateString : MonoBehaviour
    {
        public UnityEngine.UI.Text output;
        public UnityEngine.UI.InputField input;

        public void Concat()
        {
            output.text += "\n" + input.text;
        }
    }
}
#endif
