using HeathenEngineering.Events;
using HeathenEngineering.Scriptable;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [RequireComponent(typeof(UnityEngine.UI.Slider))]
    public class UixSlider : MonoBehaviour
    {
        [Header("Settings")]
        public UixSyncInitializationMode initalizeMode = UixSyncInitializationMode.MatchVariable;
        public FloatVariable valueVariable;
        [Header("Game Events")]
        public FloatGameEvent onValueChangedEvent;

        private UnityEngine.UI.Slider hostSlider;
        private bool internalUpdate = false;

        private void Awake()
        {
            hostSlider = GetComponent<UnityEngine.UI.Slider>();

            if (initalizeMode == UixSyncInitializationMode.MatchVariable && valueVariable != null)
            {
                internalUpdate = true;

                hostSlider.value = valueVariable.Value;

                internalUpdate = false;
            }
            else if (initalizeMode == UixSyncInitializationMode.MatchUiControl && valueVariable != null)
            {
                internalUpdate = true;

                valueVariable.Value = hostSlider.value;

                internalUpdate = false;
            }

            hostSlider.onValueChanged.AddListener(HandleChange);
        }

        private void OnEnable()
        {
            if (valueVariable != null)
                valueVariable.AddListener(HandleVariable);
        }

        private void OnDisable()
        {
            if (valueVariable != null)
                valueVariable.RemoveListener(HandleVariable);
        }

        private void HandleVariable(EventData<float> data)
        {
            if (internalUpdate)
                return;

            internalUpdate = true;

            hostSlider.value = data.value;

            if (onValueChangedEvent != null)
                onValueChangedEvent.Raise(valueVariable, data.value);

            internalUpdate = false;
        }

        private void HandleChange(float value)
        {
            if (internalUpdate)
                return;

            internalUpdate = true;

            if (valueVariable != null)
                valueVariable.Value = value;

            if (onValueChangedEvent != null)
                onValueChangedEvent.Raise(hostSlider, value);

            internalUpdate = false;
        }
    }
}
