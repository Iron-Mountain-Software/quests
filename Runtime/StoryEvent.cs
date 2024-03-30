using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronMountain.Conditions;
using IronMountain.SaveSystem;
using IronMountain.ScriptableActions;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests
{
    public abstract class StoryEvent : ScriptableObject
    {
        public static event Action<StoryEvent> OnAnyStateChanged;
        public event Action OnStateChanged;

        [Serializable]
        public enum StateType
        {
            Inactive = 0,
            Active = 1,
            Complete = 2,
            Failed = 3
        }
        
        [SerializeField] protected string id;

        [SerializeField] public Condition prerequisites;
        [SerializeField] public Condition completionCondition;
        [SerializeField] public Condition failCondition;
        [SerializeField] public List<ScriptableAction> actionsOnActivate = new();
        [SerializeField] public List<ScriptableAction> actionsOnComplete = new();
        [SerializeField] public List<ScriptableAction> actionsOnFail = new();
        
        private SavedInt _state;

        public StateType State
        {
            get => (StateType) _state.Value;
            private set
            {
                if (_state.Value == (int) value) return;
                _state.Value = (int) value;
            }
        }
        
        public string ID
        {
            get => id;
            private set => id = value;
        }

        protected virtual bool PrerequisitesSatisfied => !prerequisites || prerequisites.Evaluate();
        protected virtual bool CompletionConditionSatisfied => completionCondition && completionCondition.Evaluate();
        protected virtual bool FailConditionSatisfied => failCondition && failCondition.Evaluate();
        
        protected virtual string Directory => Path.Combine("Quests", ID);

        protected virtual void OnEnable()
        {
            LoadSavedData();
            Refresh();
            BroadcastSavedData();
        }

        protected virtual void OnDisable()
        {
            if (prerequisites) prerequisites.OnConditionStateChanged -= TryActivate;
            if (completionCondition) completionCondition.OnConditionStateChanged -= TryComplete;
            if (failCondition) failCondition.OnConditionStateChanged -= TryFail;
        }

        protected virtual void LoadSavedData()
        {
            _state = new SavedInt(Directory, "State.txt", 0, () =>
            {
                OnStateChanged?.Invoke();
                OnAnyStateChanged?.Invoke(this); 
            });
        }
        
        protected virtual void BroadcastSavedData()
        {
            OnStateChanged?.Invoke();
            OnAnyStateChanged?.Invoke(this);
        }

        protected void StopListening()
        {
            if (prerequisites) prerequisites.OnConditionStateChanged -= TryActivate;
            if (completionCondition) completionCondition.OnConditionStateChanged -= TryComplete;
            if (failCondition) failCondition.OnConditionStateChanged -= TryFail;
        }

        public abstract void Refresh();

        public bool CanActivate => State is StateType.Inactive && PrerequisitesSatisfied;
        public bool CanComplete => State is StateType.Active && CompletionConditionSatisfied;
        public bool CanFail => State is StateType.Active && FailConditionSatisfied;

        public void TryActivate()
        {
            if (CanActivate) Activate();
        }
        
        public void TryComplete()
        {
            if (CanComplete) Complete();
        }
        
        public void TryFail()
        {
            if (CanFail) Fail();
        }
        
        [ContextMenu("Restart", false, 0)]
        public virtual void Restart()
        {
            StopListening();
            State = StateType.Inactive;
            if (PrerequisitesSatisfied) Activate();
            else
            {
                if (prerequisites) prerequisites.OnConditionStateChanged += TryActivate;
            }
        }

        [ContextMenu("Activate", false, 0)]
        public virtual void Activate()
        {
            StopListening();
            State = StateType.Active;
            foreach (ScriptableAction action in actionsOnActivate)
                if (action) action.Invoke();
            if (CompletionConditionSatisfied) Complete();
            else if (FailConditionSatisfied) Fail();
            else
            {
                if (completionCondition) completionCondition.OnConditionStateChanged += TryComplete;
                if (failCondition) failCondition.OnConditionStateChanged += TryFail;
            }
        }
        
        [ContextMenu("Complete", false, 0)]
        public virtual void Complete()
        {
            StopListening();
            State = StateType.Complete;
            foreach (ScriptableAction action in actionsOnComplete)
                if (action) action.Invoke();
        }
        
        [ContextMenu("Fail", false, 0)]
        public virtual void Fail()
        {
            StopListening();
            State = StateType.Failed;
            foreach (ScriptableAction action in actionsOnFail)
                if (action) action.Invoke();
        }

#if UNITY_EDITOR
        
        [ContextMenu("Generate New ID")]
        protected void GenerateNewID()
        {
            ID = GUID.Generate().ToString();
        }
        
        public virtual void OnValidate()
        {
            PruneActions(ref actionsOnActivate);
            PruneActions(ref actionsOnComplete);
            PruneActions(ref actionsOnFail);
            Refresh();
        }
        
        private void PruneActions(ref List<ScriptableAction> actions)
        {
            if (actions == null || actions.Count == 0) return;
            actions = actions.Distinct().ToList();
            actions.RemoveAll(action => !action);
        }
        
        [ContextMenu("Copy Name")]
        private void CopyName()
        {
            EditorGUIUtility.systemCopyBuffer = name; 
        }
        
        public bool PrerequisitesHaveErrors => prerequisites && prerequisites.HasErrors();
        
        public bool CompletionConditionHasErrors => !completionCondition || completionCondition.HasErrors();
        
        public bool FailConditionHasErrors => failCondition && failCondition.HasErrors();

        public virtual bool HasErrors() =>
            string.IsNullOrWhiteSpace(ID)
            || PrerequisitesHaveErrors
            || CompletionConditionHasErrors
            || FailConditionHasErrors;

        public abstract string GetDocumentation();
#endif
    }
}