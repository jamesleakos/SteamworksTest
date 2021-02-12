using HeathenEngineering.Events;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [CreateAssetMenu(menuName = "UIX/Cursor/State")]
    public class UixCursorState : GameEvent<bool>
    {
        public Vector2 hotSpot = Vector2.zero;
        public UixCursorAnimation Animation;
    }
}
