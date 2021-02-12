using HeathenEngineering.Scriptable;
using HeathenEngineering.Tools;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [AddComponentMenu("Tools/UIX/Proximity Scale")]
    public class ProximityScale : ProximitySensor
    {
        public AnimationCurve ScaleCurve;

        private void Update()
        {
            Refresh();
            selfTransform.localScale = Vector3.one * ScaleCurve.Evaluate(Value);
        }
    }
}
