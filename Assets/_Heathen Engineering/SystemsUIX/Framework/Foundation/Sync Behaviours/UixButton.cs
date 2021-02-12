using HeathenEngineering.Events;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class UixButton : MonoBehaviour
    {
        [Header("Game Events")]
        public GameEvent onClickEvent;

        private UnityEngine.UI.Button hostButton;

        private void Awake()
        {
            hostButton = GetComponent<UnityEngine.UI.Button>();
            hostButton.onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            if (onClickEvent != null)
                onClickEvent.Invoke(hostButton);
        }
    }
}
