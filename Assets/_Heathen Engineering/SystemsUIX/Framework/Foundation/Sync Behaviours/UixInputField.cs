using HeathenEngineering.Events;
using HeathenEngineering.Scriptable;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [RequireComponent(typeof(UnityEngine.UI.InputField))]
    public class UixInputField : MonoBehaviour
    {
        [Header("Settings")]
        public UixSyncInitializationMode initalizeMode = UixSyncInitializationMode.MatchVariable;
        public StringVariable textVariable;
        [Header("Game Events")]
        public StringGameEvent onValueChangedEvent;
        public StringGameEvent onEndEditEvent;

        private UnityEngine.UI.InputField hostInputField;
        private bool internalUpdate = false;

        private void Awake()
        {
            hostInputField = GetComponent<UnityEngine.UI.InputField>();

            if (initalizeMode == UixSyncInitializationMode.MatchVariable && textVariable != null)
            {
                internalUpdate = true;

                hostInputField.text = textVariable.Value;

                internalUpdate = false;
            }
            else if (initalizeMode == UixSyncInitializationMode.MatchUiControl && textVariable != null)
            {
                internalUpdate = true;

                textVariable.Value = hostInputField.text;

                internalUpdate = false;
            }

            hostInputField.onValueChanged.AddListener(HandleChange);
            hostInputField.onEndEdit.AddListener(HandleEndEdit);
        }

        private void OnEnable()
        {
            if (textVariable != null)
                textVariable.AddListener(HandleVariable);
        }

        private void OnDisable()
        {
            if (textVariable != null)
                textVariable.RemoveListener(HandleVariable);
        }

        private void HandleEndEdit(string value)
        {
            if (internalUpdate)
                return;

            internalUpdate = true;

            if (textVariable != null)
                textVariable.Value = value;

            if (onEndEditEvent != null)
                onEndEditEvent.Invoke(hostInputField, value);

            internalUpdate = false;
        }

        private void HandleChange(string value)
        {
            if (internalUpdate)
                return;

            internalUpdate = true;

            if (textVariable != null)
                textVariable.Value = value;

            if (onValueChangedEvent != null)
                onValueChangedEvent.Invoke(hostInputField, value);

            internalUpdate = false;
        }

        private void HandleVariable(EventData<string> data)
        {
            if (internalUpdate)
                return;

            internalUpdate = true;

            hostInputField.text = data.value;

            internalUpdate = false;
        }
    }
}
