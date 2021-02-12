using HeathenEngineering.Events;
using HeathenEngineering.Scriptable;
using System;
using UnityEngine;

namespace HeathenEngineering.UIX
{

    [RequireComponent(typeof(UnityEngine.UI.Toggle))]
    public class UixToggle : MonoBehaviour
    {
        [Header("Settings")]
        public UixSyncInitializationMode initalizeMode = UixSyncInitializationMode.MatchVariable;
        public BoolVariable isOnVariable;
        [Header("Game Events")]
        public BoolGameEvent onValueChangedEvent;

        private UnityEngine.UI.Toggle hostToggle;
        private bool internalUpdate = false;

        private void Awake()
        {
            hostToggle = GetComponent<UnityEngine.UI.Toggle>();

            if (initalizeMode == UixSyncInitializationMode.MatchVariable && isOnVariable != null)
            {
                internalUpdate = true;

                hostToggle.isOn = isOnVariable.Value;

                internalUpdate = false;
            }
            else if (initalizeMode == UixSyncInitializationMode.MatchUiControl && isOnVariable != null)
            {
                internalUpdate = true;

                isOnVariable.Value = hostToggle.isOn;

                internalUpdate = false;
            }

            hostToggle.onValueChanged.AddListener(HandleChange);
        }

        private void OnEnable()
        {
            if (isOnVariable != null)
                isOnVariable.AddListener(HandleVariable);
        }

        private void OnDisable()
        {
            if (isOnVariable != null)
                isOnVariable.RemoveListener(HandleVariable);
        }

        private void HandleVariable(EventData<bool> data)
        {
            if (internalUpdate)
                return;

            internalUpdate = true;

            hostToggle.isOn = data.value;

            if (onValueChangedEvent != null)
                onValueChangedEvent.Raise(isOnVariable, data.value);

            internalUpdate = false;
        }

        private void HandleChange(bool value)
        {
            if (internalUpdate)
                return;

            internalUpdate = true;

            if (isOnVariable != null)
                isOnVariable.Value = value;

            if (onValueChangedEvent != null)
                onValueChangedEvent.Raise(hostToggle, value);

            internalUpdate = false;
        }
    }
}
