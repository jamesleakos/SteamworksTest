using HeathenEngineering.Events;
using HeathenEngineering.Scriptable;
using HeathenEngineering.Serializable;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    [ExecuteInEditMode]
    public class UixText : UixSyncTool
    {
        [Header("Settings")]
        public FontPointerVariable fontVariable;
        public IntVariable fontSizeVariable;
        public UguiFontStylePointerVariable fontStyleVariable;
        public StringVariable textVariable;
        public ColorVariable colorVariable;

        private UnityEngine.UI.Text hostText;
        private bool internalUpdate = false;

        private void Awake()
        {

            hostText = GetComponent<UnityEngine.UI.Text>();

            if (initalizeMode == UixSyncInitializationMode.MatchVariable)
            {
                SetObjectFromVariables();
            }
            else if (initalizeMode == UixSyncInitializationMode.MatchUiControl)
            {
                SetVariablesFromObject();
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

            if (fontSizeVariable != null)
                fontSizeVariable.AddListener(HandleFontSize);

            if (fontStyleVariable != null)
                fontStyleVariable.AddListener(HandleFontStyle);
        }

        private void OnDisable()
        {
            if (fontVariable != null)
                fontVariable.RemoveListener(HandleFont);

            if (textVariable != null)
                textVariable.RemoveListener(HandleText);

            if (colorVariable != null)
                colorVariable.RemoveListener(HandleColor);

            if (fontSizeVariable != null)
                fontSizeVariable.RemoveListener(HandleFontSize);

            if (fontStyleVariable != null)
                fontStyleVariable.RemoveListener(HandleFontStyle);
        }

        private void HandleFontStyle(EventData<FontStyle> data)
        {
            if (internalUpdate)
                return;

            hostText.fontStyle = data.value;
        }

        private void HandleFontSize(EventData<int> data)
        {
            if (internalUpdate)
                return;

            hostText.fontSize = data.value;
        }

        private void HandleFont(EventData<Font> data)
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

        [ContextMenu(nameof(SetObjectFromVariables))]
        public override void SetObjectFromVariables()
        {
            if (hostText == null)
                hostText = GetComponent<UnityEngine.UI.Text>();
            if (hostText == null)
                return;

            internalUpdate = true;

            if (fontVariable != null)
                hostText.font = fontVariable.Value;
            if (textVariable != null)
                hostText.text = textVariable.Value;
            if (colorVariable != null)
                hostText.color = colorVariable.Value;
            if (fontSizeVariable != null)
                hostText.fontSize = fontSizeVariable.Value;
            if (fontStyleVariable != null)
                hostText.fontStyle = fontStyleVariable.Value;

            internalUpdate = false;
        }

        [ContextMenu(nameof(SetVariablesFromObject))]
        public override void SetVariablesFromObject()
        {
            if (hostText == null)
                hostText = GetComponent<UnityEngine.UI.Text>();
            if (hostText == null)
                return;

            internalUpdate = true;

            if (fontVariable != null)
                fontVariable.Value = hostText.font;
            if (textVariable != null)
                textVariable.Value = hostText.text;
            if (colorVariable != null)
                colorVariable.Value = hostText.color;
            if (fontSizeVariable != null)
                fontSizeVariable.Value = hostText.fontSize;
            if (fontStyleVariable != null)
                fontStyleVariable.Value = hostText.fontStyle;

            internalUpdate = false;
        }
    }
}
