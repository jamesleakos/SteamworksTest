using HeathenEngineering.Events;
using HeathenEngineering.Scriptable;
using HeathenEngineering.Serializable;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeathenEngineering.UIX
{

    /// <summary>
    /// Handles syncing of data to and from variables and <see cref="UnityEngine.UI.Image"/> attributes.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    [ExecuteInEditMode]
    public class UixImage : UixSyncTool
    {
        /// <summary>
        /// The sprite value of the image
        /// </summary>
        public SpritePointerVariable spriteVariable;
        /// <summary>
        /// The main color value of the image
        /// </summary>
        public ColorVariable colorVariable;
        /// <summary>
        /// Rather or not the image is a raycast target
        /// </summary>
        public BoolVariable raycastTargetVariable;
        /// <summary>
        /// The fill amount of the image
        /// </summary>
        public FloatVariable fillAmountVariable;
        /// <summary>
        /// The type of image this is
        /// </summary>
        public ImageTypePointerVariable imageTypeVariable;

        private UnityEngine.UI.Image hostImage;
        private bool internalUpdate = false;

        private void Awake()
        {
            hostImage = GetComponent<UnityEngine.UI.Image>();

            if (initalizeMode == UixSyncInitializationMode.MatchVariable)
            {
                internalUpdate = true;

                if(spriteVariable != null)
                    hostImage.sprite = spriteVariable.Value;
                if(colorVariable != null)
                    hostImage.color = colorVariable.Value;
                if (fillAmountVariable != null)
                    hostImage.fillAmount = fillAmountVariable.Value;
                if (raycastTargetVariable != null)
                    hostImage.raycastTarget = raycastTargetVariable.Value;
                if (imageTypeVariable != null)
                    hostImage.type = imageTypeVariable.Value;

                internalUpdate = false;
            }
            else if (initalizeMode == UixSyncInitializationMode.MatchUiControl)
            {
                internalUpdate = true;

                if (spriteVariable != null)
                    spriteVariable.Value = hostImage.sprite;
                if (colorVariable != null)
                    colorVariable.Value = hostImage.color;
                if (fillAmountVariable != null)
                    fillAmountVariable.Value = hostImage.fillAmount;
                if (raycastTargetVariable != null)
                    raycastTargetVariable.Value = hostImage.raycastTarget;
                if (imageTypeVariable != null)
                    imageTypeVariable.Value = hostImage.type;

                internalUpdate = false;
            }
        }

        [ContextMenu(nameof(SetObjectFromVariables))]
        public override void SetObjectFromVariables()
        {
            if (hostImage == null)
                hostImage = GetComponent<UnityEngine.UI.Image>();
            if (hostImage == null)
                return;

            internalUpdate = true;

            if (spriteVariable != null)
                hostImage.sprite = spriteVariable.Value;
            if (colorVariable != null)
                hostImage.color = colorVariable.Value;
            if (fillAmountVariable != null)
                hostImage.fillAmount = fillAmountVariable.Value;
            if (raycastTargetVariable != null)
                hostImage.raycastTarget = raycastTargetVariable.Value;
            if (imageTypeVariable != null)
                hostImage.type = imageTypeVariable.Value;

            internalUpdate = false;
        }

        [ContextMenu(nameof(SetVariablesFromObject))]
        public override void SetVariablesFromObject()
        {
            if (hostImage == null)
                hostImage = GetComponent<UnityEngine.UI.Image>();
            if (hostImage == null)
                return;

            internalUpdate = true;

            if (spriteVariable != null)
                spriteVariable.Value = hostImage.sprite;
            if (colorVariable != null)
                colorVariable.Value = hostImage.color;
            if (fillAmountVariable != null)
                fillAmountVariable.Value = hostImage.fillAmount;
            if (raycastTargetVariable != null)
                raycastTargetVariable.Value = hostImage.raycastTarget;
            if (imageTypeVariable != null)
                imageTypeVariable.Value = hostImage.type;

            internalUpdate = false;
        }

        private void OnEnable()
        {
            if (spriteVariable != null)
                spriteVariable.AddListener(HandleSprite);

            if (colorVariable != null)
                colorVariable.AddListener(HandleColor);

            if (fillAmountVariable != null)
                fillAmountVariable.AddListener(HandleFillAmount);

            if (raycastTargetVariable != null)
                raycastTargetVariable.AddListener(HandleRaycast);

            if (imageTypeVariable != null)
                imageTypeVariable.AddListener(HandleImageType);
        }

        private void OnDisable()
        {
            if (spriteVariable != null)
                spriteVariable.RemoveListener(HandleSprite);

            if (colorVariable != null)
                colorVariable.RemoveListener(HandleColor);

            if (fillAmountVariable != null)
                fillAmountVariable.RemoveListener(HandleFillAmount);

            if (raycastTargetVariable != null)
                raycastTargetVariable.RemoveListener(HandleRaycast);

            if (imageTypeVariable != null)
                imageTypeVariable.RemoveListener(HandleImageType);
        }

        private void HandleImageType(EventData<Image.Type> data)
        {
            if (internalUpdate)
                return;

            hostImage.type = data.value;
        }

        private void HandleRaycast(EventData<bool> data)
        {
            if (internalUpdate)
                return;

            hostImage.raycastTarget = data.value;
        }

        private void HandleFillAmount(EventData<float> data)
        {
            if (internalUpdate)
                return;

            hostImage.fillAmount = data.value;
        }

        private void HandleColor(EventData<SerializableColor> data)
        {
            if (internalUpdate)
                return;

            hostImage.color = data.value;
        }

        private void HandleSprite(EventData<Sprite> data)
        {
            if (internalUpdate)
                return;

            hostImage.sprite = data.value;
        }
    }
}
