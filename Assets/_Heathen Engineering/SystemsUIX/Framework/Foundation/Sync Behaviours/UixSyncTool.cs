using UnityEngine;

namespace HeathenEngineering.UIX
{
    public abstract class UixSyncTool : MonoBehaviour
    {
        #if UNITY_EDITOR
        public bool AutoSyncInEditor = false;
        #endif

        /// <summary>
        /// Defines how this object should initialize e.g. load variable values or set them from the state of the image
        /// </summary>
        [Header("Settings")]
        public UixSyncInitializationMode initalizeMode = UixSyncInitializationMode.MatchVariable;
        
        public abstract void SetObjectFromVariables();

        public abstract void SetVariablesFromObject();

        #if UNITY_EDITOR
        private void Update()
        {
            if (AutoSyncInEditor)
                SetObjectFromVariables();
        }
        #endif
    }
}
