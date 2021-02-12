using UnityEngine;

namespace HeathenEngineering.UIX
{
    [System.Serializable]
    public class UixCursorAnimation
    {
        public bool loop = false;
        public Texture2D[] textureArray;
        public float framesPerSecond = 30;
    }
}
