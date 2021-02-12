using UnityEngine;

namespace HeathenEngineering.UIX
{
    public class UixCursorAnimator : MonoBehaviour
    {
        public UixCursorSettings settings;

        public void LateUpdate()
        {
            settings.Apply(Time.unscaledDeltaTime);
        }
    }
}
