using HeathenEngineering.Events;
using HeathenEngineering.Scriptable;
using HeathenEngineering.Serializable;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [RequireComponent(typeof(TMPro.TextMeshProUGUI))]
    public class UixTextMeshProUGUI : MonoBehaviour
    {
        [Header("Settings")]
        public UixSyncInitializationMode initalizeMode = UixSyncInitializationMode.MatchVariable;
        public TMProFontPointerVariable fontVariable;
        public StringVariable textVariable;
        public ColorVariable colorVariable;

        private TMPro.TextMeshProUGUI hostText;
        private bool internalUpdate = false;

        private void Awake()
        {
            hostText = GetComponent<TMPro.TextMeshProUGUI>();

            if (initalizeMode == UixSyncInitializationMode.MatchVariable)
            {
                internalUpdate = true;

                if (fontVariable != null)
                    hostText.font = fontVariable.Value;
                if (textVariable != null)
                    hostText.text = textVariable.Value;
                if (colorVariable != null)
                    hostText.color = colorVariable.Value;

                internalUpdate = false;
            }
            else if (initalizeMode == UixSyncInitializationMode.MatchUiControl)
            {
                internalUpdate = true;

                if (fontVariable != null)
                    fontVariable.Value = hostText.font;
                if (textVariable != null)
                    textVariable.Value = hostText.text;
                if (colorVariable != null)
                    colorVariable.Value = hostText.color;

                internalUpdate = false;
            }
        }

        private void OnEnable()
        {
            if (fontVariable != null)
                fontVariable.AddListener(HandleFont);

            if (textVariable != null)
                textVariable.AddListener(HandleText);

            if (colorVariable != null)
                colorVariable.AddListener(HandleColor);
        }

        private void OnDisable()
        {
            if (fontVariable != null)
                fontVariable.RemoveListener(HandleFont);

            if (textVariable != null)
                textVariable.RemoveListener(HandleText);

            if (colorVariable != null)
                colorVariable.RemoveListener(HandleColor);
        }

        private void HandleFont(EventData<TMPro.TMP_FontAsset> data)
        {
            if (internalUpdate)
                return;

            hostText.font = data.value;
        }

        private void HandleColor(EventData<SerializableColor> data)
        {
            if (internalUpdate)
                return;

            hostText.color = data.value;
        }

        private void HandleText(EventData<string> data)
        {
            if (internalUpdate)
                return;

            hostText.text = data.value;
        }
    }
}
