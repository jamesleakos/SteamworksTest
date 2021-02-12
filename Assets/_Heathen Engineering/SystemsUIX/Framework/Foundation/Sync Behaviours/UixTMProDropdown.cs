using HeathenEngineering.Events;
using HeathenEngineering.Scriptable;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [RequireComponent(typeof(TMPro.TMP_Dropdown))]
    public class UixTMProDropdown : MonoBehaviour
    {
        [Header("Settings")]
        public UixSyncInitializationMode initalizeMode = UixSyncInitializationMode.MatchVariable;
        public IntVariable valueVariable;
        public TMProDropdownOptionDataList optionVariable;
        [Header("Game Events")]
        public IntGameEvent onValueChangedEvent;

        private TMPro.TMP_Dropdown hostDropdown;
        private bool internalUpdate = false;

        private void Awake()
        {
            hostDropdown = GetComponent<TMPro.TMP_Dropdown>();

            if (initalizeMode == UixSyncInitializationMode.MatchVariable)
            {
                internalUpdate = true;

                if (valueVariable != null)
                    hostDropdown.value = valueVariable.Value;

                if (optionVariable != null)
                {
                    hostDropdown.options.Clear();
                    hostDropdown.options.AddRange(optionVariable.Value);
                }

                hostDropdown.RefreshShownValue();

                internalUpdate = false;
            }
            else if (initalizeMode == UixSyncInitializationMode.MatchUiControl && valueVariable != null)
            {
                internalUpdate = true;

                if (valueVariable != null)
                    valueVariable.Value = hostDropdown.value;

                if (optionVariable != null)
                    optionVariable.Value = hostDropdown.options;

                internalUpdate = false;
            }

            hostDropdown.onValueChanged.AddListener(HandleChange);
        }

        private void OnEnable()
        {
            if (valueVariable != null)
                valueVariable.AddListener(HandleVariable);

            if (optionVariable != null)
                optionVariable.AddListener(HandleOptions);
        }

        private void OnDisable()
        {
            if (valueVariable != null)
                valueVariable.RemoveListener(HandleVariable);

            if (optionVariable != null)
                optionVariable.RemoveListener(HandleOptions);
        }

        private void HandleOptions(CollectionChangeEventData<TMPro.TMP_Dropdown.OptionData> data)
        {
            if (internalUpdate)
                return;

            internalUpdate = true;

            hostDropdown.options.Clear();
            hostDropdown.options.AddRange(optionVariable.Value);

            internalUpdate = false;
        }

        private void HandleVariable(EventData<int> data)
        {
            if (internalUpdate)
                return;

            internalUpdate = true;

            hostDropdown.value = data.value;

            if (onValueChangedEvent != null)
                onValueChangedEvent.Raise(valueVariable, data.value);

            internalUpdate = false;
        }

        private void HandleChange(int value)
        {
            if (internalUpdate)
                return;

            internalUpdate = true;

            if (valueVariable != null)
                valueVariable.Value = value;

            if (onValueChangedEvent != null)
                onValueChangedEvent.Raise(hostDropdown, value);

            internalUpdate = false;
        }
    }
}
