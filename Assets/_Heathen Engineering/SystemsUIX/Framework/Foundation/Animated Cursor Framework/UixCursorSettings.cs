using UnityEngine;

namespace HeathenEngineering.UIX
{
    [CreateAssetMenu(menuName = "UIX/Cursor/Settings")]
    public class UixCursorSettings : ScriptableObject
    {
        public UixCursorState DefaultState;
        public UixCursorState CurrentState;

        public bool visible = true;
        public CursorLockMode lockMode = CursorLockMode.None;
        public CursorMode cursorMode = CursorMode.Auto;

        private int currentFrame;
        private float frameTimer;

        public void Apply(float deltaTime)
        {
            if (CurrentState == null)
                CurrentState = DefaultState;

            if (CurrentState == null)
            {
                Debug.LogError("Attempted to apply cursor settings with no current state nor default sate ... no actions taken!");
                return;
            }

            Cursor.visible = visible;
            Cursor.lockState = lockMode;

            frameTimer -= deltaTime;
            if (frameTimer <= 0f)
            {
                frameTimer += (1f / CurrentState.Animation.framesPerSecond);
                Cursor.SetCursor(CurrentState.Animation.textureArray[currentFrame], CurrentState.hotSpot, cursorMode);

                if(CurrentState.Animation.loop || currentFrame < CurrentState.Animation.textureArray.Length - 1)
                currentFrame = (currentFrame + 1) % CurrentState.Animation.textureArray.Length;
            }
        }

        public void SetDefault()
        {
            SetState(DefaultState);
        }

        public void SetState(UixCursorState state)
        {
            var pState = CurrentState;
            CurrentState = state;

            currentFrame = 0;
            frameTimer = (1f / CurrentState.Animation.framesPerSecond);

            Cursor.SetCursor(CurrentState.Animation.textureArray[currentFrame], CurrentState.hotSpot, cursorMode);

            if (pState != null && pState != state)
                pState.Invoke(this, false);

            if (state != null)
                state.Invoke(this, true);
        }
    }
}
