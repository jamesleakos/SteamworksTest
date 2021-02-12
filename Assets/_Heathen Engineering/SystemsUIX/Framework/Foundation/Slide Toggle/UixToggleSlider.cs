using HeathenEngineering.Scriptable;
using UnityEngine;
using UnityEngine.UI;

namespace HeathenEngineering.UIX.Controls
{
    [RequireComponent(typeof(Toggle))]
    public class UixToggleSlider : MonoBehaviour
    {
        public RectTransform slider;
        public Image image;
        public Vector2 onPosition;
        public Vector2 offPosition;
        public ColorReference onColor;
        public ColorReference offColor;
        public FloatReference transationRate;

        private Toggle hostToggle;

        private void Start()
        {
            hostToggle = GetComponent<Toggle>();
        }

        private void Update()
        {
            if (slider == null || image == null)
                return;

            float dTime = Time.unscaledDeltaTime;

            if (hostToggle.isOn)
            {
                if (slider.anchoredPosition != onPosition)
                {
                    slider.anchoredPosition = Vector2.Lerp(slider.anchoredPosition, onPosition, dTime * (1f / transationRate));
                }

                if (image.color != onColor)
                {
                    image.color = Color.Lerp(image.color, onColor.Value, dTime * (1f / transationRate));
                }
            }
            else
            {
                if (slider.anchoredPosition != offPosition)
                {
                    slider.anchoredPosition = Vector2.Lerp(slider.anchoredPosition, offPosition, dTime * (1f / transationRate));
                }

                if (image.color != offColor)
                {
                    image.color = Color.Lerp(image.color, offColor.Value, dTime * (1f / transationRate));
                }
            }
        }
    }
}


