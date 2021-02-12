#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using UnityEngine;
using HeathenEngineering.Tools;
using HeathenEngineering.Scriptable;
using UnityEngine.Serialization;

namespace HeathenEngineering.SteamApi.Foundation.UI
{
    /// <summary>
    /// <para>A composit control for displaying the avatar, name and status of a given <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> object.</para>
    /// </summary>
    public class SteamUserFullIcon : HeathenUIBehaviour
    {
        /// <summary>
        /// The <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> to load.
        /// This should be set by calling <see cref="HeathenEngineering.SteamApi.Foundation.UI.SteamUserFullIcon.LinkSteamUser(SteamUserData)"/>
        /// </summary>
        [FormerlySerializedAs("UserData")]
        public SteamUserData userData;
        /// <summary>
        /// Should the status label be shown or not
        /// </summary>
        [FormerlySerializedAs("ShowStatusLabel")]
        public BoolReference showStatusLabel;

        /// <summary>
        /// The image to load the avatar into.
        /// </summary>
        [Header("References")]
        [FormerlySerializedAs("Avatar")]
        public UnityEngine.UI.RawImage avatar;
        /// <summary>
        /// The text field used to display the users name
        /// </summary>
        [FormerlySerializedAs("PersonaName")]
        public UnityEngine.UI.Text personaName;
        /// <summary>
        /// The text field used to display the users status
        /// </summary>
        [FormerlySerializedAs("StatusLabel")]
        public UnityEngine.UI.Text statusLabel;
        /// <summary>
        /// An image board around the icon ... this will have its color changed based on status
        /// </summary>
        [FormerlySerializedAs("IconBorder")]
        public UnityEngine.UI.Image iconBorder;
        /// <summary>
        /// The root object containing the status label parts ... this is what is enabled or disabled as the label is shown or hidden.
        /// </summary>
        [FormerlySerializedAs("StatusLabelContainer")]
        public GameObject statusLabelContainer;
        /// <summary>
        /// Should the persona name be colored based on status
        /// </summary>
        [FormerlySerializedAs("ColorThePersonaName")]
        public bool colorThePersonaName = true;
        /// <summary>
        /// Should the status label be colored based on status
        /// </summary>
        [FormerlySerializedAs("ColorTheStatusLabel")]
        public bool colorTheStatusLabel = true;
        /// <summary>
        /// <para></para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        [Header("Border Colors")]
        [FormerlySerializedAs("OfflineColor")]
        public ColorReference offlineColor;
        /// <summary>
        /// <para>The color to use for Online</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        [FormerlySerializedAs("OnlineColor")]
        public ColorReference onlineColor;
        /// <summary>
        /// <para>The color to use for Away</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        [FormerlySerializedAs("AwayColor")]
        public ColorReference awayColor;
        /// <summary>
        /// <para>The color to use for Buisy</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        [FormerlySerializedAs("BuisyColor")]
        public ColorReference buisyColor;
        /// <summary>
        /// <para>The color to use for Snooze</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        [FormerlySerializedAs("SnoozeColor")]
        public ColorReference snoozeColor;
        /// <summary>
        /// <para>The color to use for the Want to Play status</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        [FormerlySerializedAs("WantPlayColor")]
        public ColorReference wantPlayColor;
        /// <summary>
        /// <para>The color to use for the Want to Trade status</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        [FormerlySerializedAs("WantTradeColor")]
        public ColorReference wantTradeColor;
        /// <summary>
        /// <para>Color to use for In Game satus</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        [FormerlySerializedAs("InGameColor")]
        public ColorReference inGameColor;
        /// <summary>
        /// <para>The color to use when in this specific game</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        [FormerlySerializedAs("ThisGameColor")]
        public ColorReference thisGameColor;

        private void Start()
        {
            if (userData != null)
                LinkSteamUser(userData);
        }

        private void Update()
        {
            if (showStatusLabel.Value != statusLabelContainer.activeSelf)
                statusLabelContainer.SetActive(showStatusLabel.Value);
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
        /// myUserFullIcon.LinkSteamUser(settings.UserData);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void LinkSteamUser(SteamUserData newUserData)
        {
            if (userData != null)
            {
                if (userData.OnAvatarChanged != null)
                    userData.OnAvatarChanged.RemoveListener(handleAvatarChange);
                if (userData.OnStateChange != null)
                    userData.OnStateChange.RemoveListener(handleStateChange);
                if (userData.OnNameChanged != null)
                    userData.OnNameChanged.RemoveListener(handleNameChanged);
                if (userData.OnAvatarLoaded != null)
                    userData.OnAvatarLoaded.RemoveListener(handleAvatarChange);
            }

            userData = newUserData;
            handleAvatarChange();
            handleNameChanged();
            handleStateChange();

            if (userData != null)
            {
                if (!userData.iconLoaded)
                    SteamSettings.current.client.RefreshAvatar(userData);

                avatar.texture = userData.avatar;
                if (userData.OnAvatarChanged == null)
                    userData.OnAvatarChanged = new UnityEngine.Events.UnityEvent();
                userData.OnAvatarChanged.AddListener(handleAvatarChange);
                if (userData.OnStateChange == null)
                    userData.OnStateChange = new UnityEngine.Events.UnityEvent();
                userData.OnStateChange.AddListener(handleStateChange);
                if (userData.OnNameChanged == null)
                    userData.OnNameChanged = new UnityEngine.Events.UnityEvent();
                userData.OnNameChanged.AddListener(handleNameChanged);
                if (userData.OnAvatarLoaded == null)
                    userData.OnAvatarLoaded = new UnityEngine.Events.UnityEvent();
                userData.OnAvatarLoaded.AddListener(handleAvatarChange);
            }
        }

        private void handleNameChanged()
        {
            personaName.text = userData.DisplayName;
        }

        private void handleAvatarChange()
        {
            avatar.texture = userData.avatar;
        }

        private void handleStateChange()
        {
            switch(userData.State)
            {
                case Steamworks.EPersonaState.k_EPersonaStateAway:
                    if (userData.InGame)
                    {
                        if (userData.GameInfo.m_gameID.AppID().m_AppId == SteamSettings.current.applicationId.m_AppId)
                        {
                            statusLabel.text = "Playing";
                            iconBorder.color = thisGameColor.Value;
                        }
                        else
                        {
                            statusLabel.text = "In-Game";
                            iconBorder.color = inGameColor.Value;
                        }
                    }
                    else
                    {
                        statusLabel.text = "Away";
                        iconBorder.color = awayColor.Value;
                    }
                    break;
                case Steamworks.EPersonaState.k_EPersonaStateBusy:
                    if (userData.InGame)
                    {
                        if (userData.GameInfo.m_gameID.AppID().m_AppId == SteamSettings.current.applicationId.m_AppId)
                        {
                            statusLabel.text = "Playing";
                            iconBorder.color = thisGameColor.Value;
                        }
                        else
                        {
                            statusLabel.text = "In-Game";
                            iconBorder.color = inGameColor.Value;
                        }
                    }
                    else
                    {
                        statusLabel.text = "Buisy";
                        iconBorder.color = buisyColor.Value;
                    }
                    break;
                case Steamworks.EPersonaState.k_EPersonaStateLookingToPlay:
                    statusLabel.text = "Looking to Play";
                    iconBorder.color = wantPlayColor.Value;
                    break;
                case Steamworks.EPersonaState.k_EPersonaStateLookingToTrade:
                    statusLabel.text = "Looking to Trade";
                    iconBorder.color = wantTradeColor.Value;
                    break;
                case Steamworks.EPersonaState.k_EPersonaStateOffline:
                    statusLabel.text = "Offline";
                    iconBorder.color = offlineColor.Value;
                    break;
                case Steamworks.EPersonaState.k_EPersonaStateOnline:
                    if (userData.InGame)
                    {
                        if (userData.GameInfo.m_gameID.AppID().m_AppId == SteamSettings.current.applicationId.m_AppId)
                        {
                            statusLabel.text = "Playing";
                            iconBorder.color = thisGameColor.Value;
                        }
                        else
                        {
                            statusLabel.text = "In-Game";
                            iconBorder.color = inGameColor.Value;
                        }
                    }
                    else
                    {
                        statusLabel.text = "Online";
                        iconBorder.color = onlineColor.Value;
                    }
                    break;
                case Steamworks.EPersonaState.k_EPersonaStateSnooze:
                    if (userData.InGame)
                    {
                        if (userData.GameInfo.m_gameID.AppID().m_AppId == SteamSettings.current.applicationId.m_AppId)
                        {
                            statusLabel.text = "Playing";
                            iconBorder.color = thisGameColor.Value;
                        }
                        else
                        {
                            statusLabel.text = "In-Game";
                            iconBorder.color = inGameColor.Value;
                        }
                    }
                    else
                    {
                        statusLabel.text = "Snooze";
                        iconBorder.color = snoozeColor.Value;
                    }
                    break;
            }
            if (colorTheStatusLabel)
                statusLabel.color = iconBorder.color;
            if (colorThePersonaName)
                personaName.color = iconBorder.color;
        }

        private void OnDestroy()
        {
            if (userData != null)
                userData.OnAvatarChanged.RemoveListener(handleAvatarChange);
        }
    }
}
#endif