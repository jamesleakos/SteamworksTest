using HeathenEngineering.Events;
using HeathenEngineering.Scriptable;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [RequireComponent(typeof(UnityEngine.UI.Dropdown))]
    public class UixDropdown : UixSyncTool
    {
        public IntVariable valueVariable;
        public DropdownOptionDataList optionVariable;
        [Header("Game Events")]
        public IntGameEvent onValueChangedEvent;

        private UnityEngine.UI.Dropdown hostDropdown;
        private bool internalUpdate = false;

        private void Awake()
        {
            hostDropdown = GetComponent<UnityEngine.UI.Dropdown>();

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

        private void HandleOptions(CollectionChangeEventData<UnityEngine.UI.Dropdown.OptionData> data)
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

        [ContextMenu(nameof(SetObjectFromVariables))]
        public override void SetObjectFromVariables()
        {
            if(hostDropdown == null)
                hostDropdown = GetComponent<UnityEngine.UI.Dropdown>();
            if (hostDropdown == null)
                return;

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

        [ContextMenu(nameof(SetVariablesFromObject))]
        public override void SetVariablesFromObject()
        {
            if (hostDropdown == null)
                hostDropdown = GetComponent<UnityEngine.UI.Dropdown>();
            if (hostDropdown == null)
                return;

            internalUpdate = true;

            if (valueVariable != null)
                valueVariable.Value = hostDropdown.value;

            if (optionVariable != null)
                optionVariable.Value = hostDropdown.options;

            internalUpdate = false;
        }
    }
}
