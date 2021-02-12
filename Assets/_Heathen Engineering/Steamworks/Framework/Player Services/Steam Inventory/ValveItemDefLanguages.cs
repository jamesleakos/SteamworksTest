#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

namespace HeathenEngineering.SteamApi.PlayerServices
{
    public enum ValveItemDefLanguages
    {
        none,
        arabic,
        bulgarian,
        schinese,
        tchinese,
        czech,
        danish,
        dutch,
        english,
        finnish,
        french,
        german,
        greek,
        hungarian,
        italian,
        japanese,
        korean,
        norwegian,
        polish,
        portuguese,
        brazilian,
        romanian,
        russian,
        spanish,
        latam,
        swedish,
        thai,
        turkish,
        ukrainian,
        vietnamese
    }
}
#endif