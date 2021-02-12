using UnityEngine;
using UnityEngine.EventSystems;

namespace HeathenEngineering.UIX
{
    public class SetUixCursorState : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UixCursorSettings settings;
        public UixCursorState stateOnEnter;

        public void OnPointerEnter(PointerEventData eventData)
        {
            settings.SetState(stateOnEnter);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            settings.SetDefault();
        }
    }
}
