using HeathenEngineering.Events;
using HeathenEngineering.Scriptable;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorEventTrigger : MonoBehaviour
    {
        [Serializable]
        public class TriggerMapping
        {
            public string triggerName;
            public UixCursorState cursorState;
            public GameEvent gameEvent;
            [HideInInspector]
            public Animator animator;

            public void Register()
            {
                if (gameEvent != null)
                    gameEvent.AddListener(Handler);

                if (cursorState != null)
                    cursorState.AddListener(HandleState);
            }

            public void Unregister()
            {
                if (gameEvent != null)
                    gameEvent.RemoveListener(Handler);

                if (cursorState != null)
                    cursorState.RemoveListener(HandleState);
            }

            private void HandleState(EventData<bool> data)
            {
                if (animator != null && data.value)
                    animator.SetTrigger(triggerName);
            }

            private void Handler(EventData data)
            {
                if (animator != null)
                    animator.SetTrigger(triggerName);
            }
        }

        [Serializable]
        public class BoolMapping
        {
            public string boolName;
            public UixCursorState cursorState;
            public BoolGameEvent gameEvent;
            public BoolVariable variable;
            [HideInInspector]
            public Animator animator;

            public void Register()
            {
                if (gameEvent != null)
                    gameEvent.AddListener(Handler);

                if (cursorState != null)
                    cursorState.AddListener(HandleState);

                if (variable != null)
                    variable.AddListener(HandleVariable);
            }

            public void Unregister()
            {
                if (gameEvent != null)
                    gameEvent.RemoveListener(Handler);

                if (cursorState != null)
                    cursorState.RemoveListener(HandleState);

                if (variable != null)
                    variable.RemoveListener(HandleVariable);
            }

            private void HandleVariable(EventData<bool> data)
            {
                if (animator != null)
                    animator.SetBool(boolName, data.value);
            }

            private void HandleState(EventData<bool> data)
            {
                if (animator != null && data.value)
                    animator.SetBool(boolName, data.value);
            }

            private void Handler(EventData<bool> data)
            {
                if (animator != null)
                    animator.SetBool(boolName, data.value);
            }
        }

        [Serializable]
        public class FloatMapping
        {
            public string floatName;
            public FloatGameEvent gameEvent;
            public FloatVariable variable;
            [HideInInspector]
            public Animator animator;

            public void Register()
            {
                if (gameEvent != null)
                    gameEvent.AddListener(Handler);

                if (variable != null)
                    variable.AddListener(HandleVariable);
            }

            public void Unregister()
            {
                if (gameEvent != null)
                    gameEvent.RemoveListener(Handler);

                if (variable != null)
                    variable.RemoveListener(HandleVariable);
            }

            private void HandleVariable(EventData<float> data)
            {
                if (animator != null)
                    animator.SetFloat(floatName, data.value);
            }

            private void Handler(EventData<float> data)
            {
                if (animator != null)
                    animator.SetFloat(floatName, data.value);
            }
        }

        public List<TriggerMapping> TriggerMap;
        public List<BoolMapping> BoolMap;
        public List<FloatMapping> FloatMap;


        private Animator animator;

        private void OnEnable()
        {
            animator = GetComponent<Animator>();

            if (animator != null)
            {
                foreach (var mapping in TriggerMap)
                {
                    mapping.animator = animator;
                    mapping.Register();
                }

                foreach (var mapping in BoolMap)
                {
                    mapping.animator = animator;
                    mapping.Register();
                }

                foreach (var mapping in FloatMap)
                {
                    mapping.animator = animator;
                    mapping.Register();
                }
            }
        }

        private void OnDisable()
        {
            animator = GetComponent<Animator>();

            if (animator != null)
            {
                foreach (var mapping in TriggerMap)
                {
                    mapping.Unregister();
                }

                foreach (var mapping in BoolMap)
                {
                    mapping.animator = animator;
                    mapping.Unregister();
                }

                foreach (var mapping in FloatMap)
                {
                    mapping.animator = animator;
                    mapping.Unregister();
                }
            }
        }
    }
}
