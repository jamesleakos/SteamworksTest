#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace HeathenEngineering.SteamApi.Foundation.UI
{
    /// <summary>
    /// Extends the <see cref="SteamUserFullIcon"/> control adding support for mouse clicks on the icon
    /// </summary>
    public class SteamUserIconButton : SteamUserFullIcon, IPointerClickHandler
    {
        /// <summary>
        /// Occures when the icon is clicked once with the left mouse button
        /// </summary>
        [FormerlySerializedAs("OnLeftClick")]
        public UnityPersonaEvent onLeftClick;
        /// <summary>
        /// Occures when the icon is clicked once with the middle mouse button
        /// </summary>
        [FormerlySerializedAs("OnMiddleClick")]
        public UnityPersonaEvent onMiddleClick;
        /// <summary>
        /// Occures when the icon is clicked once with the right mouse button
        /// </summary>
        [FormerlySerializedAs("OnRightClick")]
        public UnityPersonaEvent onRightClick;
        /// <summary>
        /// Occures when the icon is double clicked with the left mouse button
        /// </summary>
        [FormerlySerializedAs("OnLeftDoubleClick")]
        public UnityPersonaEvent onLeftDoubleClick;
        /// <summary>
        /// Occures when the icon is double clicked with the middle mouse button
        /// </summary>
        [FormerlySerializedAs("OnMiddleDoubleClick")]
        public UnityPersonaEvent onMiddleDoubleClick;
        /// <summary>
        /// Occures when the icon is double clicked with the right mouse button
        /// </summary>
        [FormerlySerializedAs("OnRightDoubleClick")]
        public UnityPersonaEvent onRightDoubleClick;

        /// <summary>
        /// handler for <see cref="IPointerClickHandler"/> see the Unity SDK for more information.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                if (eventData.clickCount > 1)
                    onLeftDoubleClick.Invoke(userData);
                else
                    onLeftClick.Invoke(userData);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (eventData.clickCount > 1)
                    onRightDoubleClick.Invoke(userData);
                else
                    onRightClick.Invoke(userData);
            }
            else if (eventData.button == PointerEventData.InputButton.Middle)
            {
                if (eventData.clickCount > 1)
                    onMiddleDoubleClick.Invoke(userData);
                else
                    onMiddleClick.Invoke(userData);
            }
        }
    }
}
#endif