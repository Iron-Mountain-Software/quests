using System;
using System.Collections.Generic;
using UnityEngine;

namespace IronMountain.Quests
{
    public class StoryEventController : MonoBehaviour
    {
        private enum TriggerType
        {
            OnAwake = 0,
            OnEnable = 1,
            OnStart = 2,
            OnDisable = 3,
            OnDestroy = 4,
        };
        
        private enum ActionType
        {
            Restart = 0,
            TryActivate = 1,
            Activate = 2,
            TryFail = 3,
            Fail = 4,
            TryComplete = 5,
            Complete = 6
        };

        [Serializable]
        private class ControllerEvent
        {
            [SerializeField] private StoryEvent storyEvent;
            [SerializeField] private ActionType action;
            [SerializeField] private TriggerType trigger;
            
            public TriggerType Trigger => trigger;
            
            public void Invoke()
            {
                if (storyEvent) return;
                switch (action)
                {
                    case ActionType.Restart:
                        storyEvent.Restart();
                        break;
                    case ActionType.TryActivate:
                        storyEvent.TryActivate();
                        break;
                    case ActionType.Activate:
                        storyEvent.Activate();
                        break;
                    case ActionType.TryFail:
                        storyEvent.TryFail();
                        break;
                    case ActionType.Fail:
                        storyEvent.Fail();
                        break;
                    case ActionType.TryComplete:
                        storyEvent.TryComplete();
                        break;
                    case ActionType.Complete:
                        storyEvent.Complete();
                        break;
                }
            }
        }

        [SerializeField] private List<ControllerEvent> controllerEvents = new ();

        private void Awake() => TriggerEvents(TriggerType.OnAwake);
        private void OnEnable() => TriggerEvents(TriggerType.OnEnable);
        private void Start() => TriggerEvents(TriggerType.OnStart);
        private void OnDisable() => TriggerEvents(TriggerType.OnDisable);
        private void OnDestroy() => TriggerEvents(TriggerType.OnDestroy);

        private void TriggerEvents(TriggerType trigger)
        {
            if (controllerEvents == null) return;
            foreach (ControllerEvent controllerEvent in controllerEvents)
            {
                if (controllerEvent == null) continue;
                if (controllerEvent.Trigger != trigger) continue;
                controllerEvent.Invoke();
            }
        }
    }
}
