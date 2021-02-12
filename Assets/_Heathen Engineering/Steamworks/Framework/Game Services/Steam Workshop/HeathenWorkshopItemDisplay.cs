#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.Tools;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HeathenEngineering.SteamApi.GameServices
{
    public class HeathenWorkshopItemDisplay : HeathenUIBehaviour, IWorkshopItemDisplay, IPointerEnterHandler, IPointerExitHandler
    {
        [HideInInspector]
        public UnityEngine.UI.RawImage PreviewImage;
        public UnityEngine.UI.Text Title;
        public Vector3 TipOffset;
        public GameObject TipRoot;
        public Transform TipTransform;
        public UnityEngine.UI.Text Description;
        public CanvasGroup toggleGroup;
        public UnityEngine.UI.Toggle Subscribed;
        public UnityEngine.UI.Image ScoreImage;

        public HeathenWorkshopReadCommunityItem Data
        {
            get;
            private set;
        }

        private bool loading = false;
        private bool hasMouse = false;
        
        public void RegisterData(HeathenWorkshopReadCommunityItem data)
        {
            loading = true;
            Data = data;
            PreviewImage.texture = Data.previewImage;
            Title.text = Data.Title;
            Subscribed.isOn = Data.IsSubscribed;
            ScoreImage.fillAmount = Data.VoteScore;

            loading = false;
        }

        private void Update()
        {
            if (PreviewImage.texture != Data.previewImage)
                PreviewImage.texture = Data.previewImage;

            if(hasMouse)
            {
                //We have the pointer so keep the tip with us even if we scroll
                TipTransform.position = selfTransform.position + TipOffset;

                toggleGroup.alpha = 1;
                toggleGroup.interactable = true;
            }
            else
            {
                toggleGroup.alpha = 0;
                toggleGroup.interactable = false;
            }
        }

        public void SetSubscribe(bool subscribed)
        {
            if (loading)
                return;

            if(subscribed)
            {
                SteamUGC.SubscribeItem(Data.FileId);
            }
            else
            {
                SteamUGC.UnsubscribeItem(Data.FileId);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hasMouse = true;
            //Locate the tip to be to our right at our level
            TipTransform.position = selfTransform.position + TipOffset;
            Description.text = Data.Description.Replace("[b]", "<b>").Replace("[/b]", "</b>").Replace("[table]", "").Replace("[tr]", "").Replace("[td]", "").Replace("[/table]", "").Replace("[/tr]", "").Replace("[/td]", "").Replace("[h1]", "<b>").Replace("[/h1]", "</b>");
            TipRoot.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hasMouse = false;
            //Hide the tip
            TipRoot.SetActive(false);
        }
    }
}
#endif