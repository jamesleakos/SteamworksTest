using HeathenEngineering.Scriptable;
using HeathenEngineering.Tools;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [AddComponentMenu("Heathen/UIX/Helpers/Keep Size on Screen")]
    public class KeepSizeOnScreen : HeathenBehaviour
    {
        [Header("Environment Refrences")]
        /// <summary>
        /// The camera which will be displaying this element
        /// </summary>
        [Tooltip("The camera which will be displaying this element")]
        public CameraReference renderingCamera = new CameraReference(null);
        /// <summary>
        /// The object to snap to if any
        /// </summary>
        [Tooltip("The object to snap to if any")]
        public TransformPointerReference snapToTransform = new TransformPointerReference(null);
        [Header("Element Behvaiour")]
        /// <summary>
        /// Rather or not the object should face the camera
        /// </summary>
        [Tooltip("Rather or not the object should face the camera")]
        public BoolReference faceCamera = new BoolReference(true);

        [Header("Variable Scale Behaviour")]
        /// <summary>
        /// At what distance and beyond should the min scale be used
        /// </summary>
        [Tooltip("At what distance and beyond should the min scale be used")]
        public float maxDistance = 20000f;
        /// <summary>
        /// The relative minimal scale
        /// </summary>
        [Tooltip("The relative minimal scale")]
        public float minScale = 0.1f;
        /// <summary>
        /// At what distance and closer should the max scale be used
        /// </summary>
        [Tooltip("At what distance and closer should the max scale be used")]
        public float minDistance = 100f;
        /// <summary>
        /// The relative max scale
        /// </summary>
        [Tooltip("The relative max scale")]
        public float maxScale = 3f;

        [HideInInspector]
        /// <summary>
        /// The camera distance from this object on the last update
        /// </summary>
        public float cameraDistance
        {
            get;
            private set;
        }

        [HideInInspector]
        /// <summary>
        /// The camera's transform; this is more efficent than renderingCamera.transform
        /// </summary>
        public Transform cameraTransform
        {
            get;
            private set;
        }

        private Camera targetCamera;
        private Vector3 initalScale;

        // Use this for initialization
        void Start()
        {
            targetCamera = renderingCamera.Value;
            cameraTransform = renderingCamera.Value.transform;
            initalScale = selfTransform.localScale;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if(targetCamera != renderingCamera.Value)
            {
                targetCamera = renderingCamera.Value;

                if (targetCamera != null)
                    cameraTransform = targetCamera.transform;
            }

            if (targetCamera == null)
                return;

            if (faceCamera)
            {
                selfTransform.rotation = Quaternion.LookRotation(selfTransform.position - cameraTransform.position);
            }

            if (snapToTransform != null)
            {
                selfTransform.position = snapToTransform.Value.position;
            }

            cameraDistance = Vector3.Distance(cameraTransform.position, selfTransform.position);
            var unitScale = cameraDistance * Mathf.Tan(Mathf.Deg2Rad * (renderingCamera.Value.fieldOfView * 0.5f));
            var distanceScale = Mathf.Lerp(maxScale, minScale, (cameraDistance - minDistance) / (maxDistance - minDistance));
            selfTransform.localScale = initalScale * unitScale * distanceScale;
        }
    }
}