#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
namespace HeathenEngineering.SteamApi.Networking.UI
{
    public interface ILobbyChatMessage
    {
        void RegisterChatMessage(LobbyChatMessageData data);
        void SetMessageText(string sender, string message);
    }
}
#endif
