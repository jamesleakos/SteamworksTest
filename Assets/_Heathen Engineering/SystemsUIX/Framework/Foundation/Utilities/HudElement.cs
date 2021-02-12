using UnityEngine;
using HeathenEngineering.Scriptable;
using HeathenEngineering.Tools;

namespace HeathenEngineering.UIX
{
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Tools/UIX/HUD Element")]
    public class HudElement : HeathenUIBehaviour
    {
        public CameraReference displayCamera;
        public FloatReference nearClipOffset = new FloatReference(1);
        public RectTransformPointerReference parentCanvas;
        public TransformReference followSubject;
        public GameObject displayContent;
             
        private Vector2 rectPosition;
        
        // Update is called once per frame
        void LateUpdate()
        {
            if (selfTransform == null || displayCamera.Value == null || followSubject == null)
                return;
            
            if (displayCamera.Value.WorldToScreenPoint(followSubject.Value.position).z > displayCamera.Value.nearClipPlane + nearClipOffset)
            {
                if (!displayContent.activeSelf)
                    displayContent.SetActive(true);
                rectPosition = RectTransformUtility.WorldToScreenPoint(displayCamera, followSubject.Value.position);
                selfTransform.anchoredPosition = rectPosition - parentCanvas.Value.sizeDelta * 0.5f;
            }  
            else if (displayContent.activeSelf)
                    displayContent.SetActive(false);
        }
    }
}
