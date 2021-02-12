#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// Indicates the type of item a specific pointer relates to
    /// </summary>
    public enum InventoryItemType
    {
        /// <summary>
        /// Item Definitions are real items e.g. represent a specifc type of item.
        /// </summary>
        ItemDefinition,
        /// <summary>
        /// Item Generators are probabilities of a given type of item and are assessed by the Steam backend to resolve into a specific item definition.
        /// </summary>
        ItemGenerator,
        TagGenerator,
        ItemBundle,
    }
}
#endif