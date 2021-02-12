using HeathenEngineering.Events;
using HeathenEngineering.Scriptable;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [RequireComponent(typeof(Canvas))]
    public class UixCanvas : MonoBehaviour
    {
        [Header("Settings")]
        public UixSyncInitializationMode initalizeMode = UixSyncInitializationMode.MatchVariable;
        public CameraPointerVariable Camera;
        public IntVariable OrderInLayer;
        public SortingLayerVariable SortingLayer;

        private Canvas hostCanvas;
        private bool internalUpdate = false;

        private void Awake()
        {
            hostCanvas = GetComponent<Canvas>();

            if (initalizeMode == UixSyncInitializationMode.MatchVariable)
            {
                internalUpdate = true;

                if (Camera != null)
                    hostCanvas.worldCamera = Camera.Value;
                if (OrderInLayer != null)
                    hostCanvas.sortingOrder = OrderInLayer.Value;
                if (SortingLayer != null)
                    hostCanvas.sortingLayerID = SortingLayer.Value.id;

                internalUpdate = false;
            }
            else if (initalizeMode == UixSyncInitializationMode.MatchUiControl)
            {
                internalUpdate = true;

                if (Camera != null)
                    Camera.Value = hostCanvas.worldCamera;
                if (OrderInLayer != null)
                    OrderInLayer.Value = hostCanvas.sortingOrder;
                if (SortingLayer != null)
                    SortingLayer.Value = hostCanvas.sortingLayerID;

                internalUpdate = false;
            }
        }

        private void OnEnable()
        {
            if (Camera != null)
                Camera.AddListener(HandleCamera);

            if (OrderInLayer != null)
                OrderInLayer.AddListener(HandleOrderInLayer);

            if (SortingLayer != null)
                SortingLayer.AddListener(HandleSortingLayer);
        }

        private void OnDisable()
        {
            if (Camera != null)
                Camera.RemoveListener(HandleCamera);

            if (OrderInLayer != null)
                OrderInLayer.RemoveListener(HandleOrderInLayer);

            if (SortingLayer != null)
                SortingLayer.RemoveListener(HandleSortingLayer);
        }

        private void HandleCamera(EventData<Camera> data)
        {
            if (internalUpdate)
                return;

            hostCanvas.worldCamera = data.value;
        }

        private void HandleOrderInLayer(EventData<int> data)
        {
            if (internalUpdate)
                return;

            hostCanvas.sortingOrder = data.value;
        }

        private void HandleSortingLayer(EventData<SortingLayerValue> data)
        {
            if (internalUpdate)
                return;

            hostCanvas.sortingLayerID = data.value.id;
        }
    }
}
