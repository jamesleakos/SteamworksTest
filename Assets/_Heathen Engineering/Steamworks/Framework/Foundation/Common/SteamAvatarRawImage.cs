#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using UnityEngine;
using HeathenEngineering.Tools;

namespace HeathenEngineering.SteamApi.Foundation.UI
{
    /// <summary>
    /// <para>Used to display the avatar image of a given <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> object.</para>
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.RawImage))]
    public class SteamAvatarRawImage : HeathenUIBehaviour
    {
        /// <summary>
        /// The image to load the avatar into.
        /// </summary>
        private UnityEngine.UI.RawImage image;
        /// <summary>
        /// The <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> to load.
        /// This should be set by calling <see cref="HeathenEngineering.SteamApi.Foundation.UI.SteamAvatarRawImage.LinkSteamUser(SteamUserData)"/>
        /// </summary>
        public SteamUserData userData;

        private void Awake()
        {
            image = GetComponent<UnityEngine.UI.RawImage>();

            LinkSteamUser(userData);
        }

        /// <summary>
        /// Sets and registeres for the provided <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> object.
        /// </summary>
        /// <param name="newUserData">The user to connect to and to display the avatar for.</param>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description>Set the icon to display the current user as read from the SteamSettings settings member.</description>
        /// <code>
        /// myAvatarRawImage.LinkSteamUser(settings.UserData);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void LinkSteamUser(SteamUserData newUserData)
        {
            if (userData != null)
                userData.OnAvatarChanged.RemoveListener(handleAvatarChange);

            userData = newUserData;

            if (userData != null)
            {
                if(image == null)
                    image = GetComponent<UnityEngine.UI.RawImage>();

                image.texture = userData.avatar;
                userData.OnAvatarChanged.AddListener(handleAvatarChange);
            }
        }

        private void handleAvatarChange()
        {
            image.texture = userData.avatar;
        }

        private void OnDestroy()
        {
            if (userData != null)
                userData.OnAvatarChanged.RemoveListener(handleAvatarChange);
        }
    }
}
#endif