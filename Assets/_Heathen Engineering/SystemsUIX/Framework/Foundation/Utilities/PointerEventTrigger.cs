using HeathenEngineering.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HeathenEngineering.UIX
{
    public class PointerEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityBoolEvent PointerEnterExitChanged;
        public UnityEvent PointerEnter;
        public UnityEvent PointerExit;

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnterExitChanged.Invoke(true);
            PointerEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerEnterExitChanged.Invoke(false);
            PointerExit.Invoke();
        }
    }
}