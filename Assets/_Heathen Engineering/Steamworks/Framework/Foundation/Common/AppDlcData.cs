#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// Represents the metadat of a DLC entry for a given app
    /// This is returned by <see cref="HeathenEngineering.SteamApi.SteamUtilities.GetDLCData"/>
    /// </summary>
    /// <example>
    /// <list type="bullet">
    /// <item>
    /// <description>Return the list of all DLC for the current application</description>
    /// <code>
    /// var results = SteamUtilities.GetDLCData();
    /// foreach(var result in results)
    /// {
    ///    Debug.Log("Located DLC " + result.name " with an AppId of " + result.appId.m_AppId.ToString() + ", this DLC is " + (result.available ? "available!" : "not available!"));
    /// }
    /// </code>
    /// </item>
    /// </list>
    /// </example>
    public struct AppDlcData
    {
        /// <summary>
        /// The application ID as defined by the Steam API
        /// </summary>
        /// <remarks>
        /// This is simply a uint with some extra belts and braces and you can easily convert to and from uint with now issue e.g.
        /// <code>
        /// var uintValue = this.m_AppId;
        /// var appIdValue = new AppId_t(uintValue);
        /// </code>
        /// </remarks>
        public AppId_t appId;
        /// <summary>
        /// The name of the DLC defined in the Steam portal for this app id.
        /// </summary>
        /// <remarks>
        /// This is the same value as you would find by calling SteamApps.BGetDLCDataByIndex on this specific DLC
        /// </remarks>
        public string name;
        /// <summary>
        /// Is the DLC listed as available
        /// </summary>
        /// <remarks>
        /// This is the same value as you would find by calling SteamApps.BGetDLCDataByIndex on this specific DLC
        /// </remarks>
        public bool available;
    }
}
#endif