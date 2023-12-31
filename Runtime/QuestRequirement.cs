using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronMountain.Conditions;
using IronMountain.SaveSystem;
using IronMountain.ScriptableActions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

namespace IronMountain.Quests
{
    public class QuestRequirement : ScriptableObject
    {
        [Serializable]
        public enum StateType
        {
            None = 0,
            Tracking = 1,
            Completed = 2,
            Failed = 3
        }
        
        [SerializeField] private string id;
        
        public string ID
        {
            get => id;
            set => id = value;
        }

        public virtual string Name => condition ? condition.ToString() : "NULL";
    
        public static event Action<QuestRequirement> OnAnyStateChanged;
        
        public event Action OnStateChanged;
        
        [SerializeField] private Quest quest;
        [SerializeField] private string defaultDetail;
        [SerializeField] private LocalizedString detail;
        [SerializeField] private string defaultTip;
        [SerializeField] private LocalizedString tip;
        [SerializeField] private List<QuestRequirement> dependencies = new ();
        [SerializeField] private List<ScriptableAction> actionsOnTrack = new ();
        [SerializeField] private List<ScriptableAction> actionsOnComplete = new ();
        [SerializeField] public Condition condition;
        [SerializeField] public Condition failCondition;
        [SerializeField] private Sprite depiction;
        
        public Quest Quest
        {
            get => quest;
            set => quest = value;
        }

        public string Detail
        {
            get
            {
                if (Application.isPlaying)
                {
                    return detail == null || detail.IsEmpty 
                        ? defaultDetail 
                        : detail.GetLocalizedString();
                }
#if UNITY_EDITOR
                if (detail == null || detail.IsEmpty || string.IsNullOrEmpty(detail.TableReference)) return defaultDetail;
                var collection =
                    UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(detail.TableReference);
                var entry = collection.SharedData.GetEntryFromReference(detail.TableEntryReference);
                return entry != null ? entry.Key : defaultDetail;
#else
				return defaultDetail;
#endif
            }
        }

        public string Tip
        {
            get
            {
                if (Application.isPlaying)
                {
                    return tip.IsEmpty ? defaultTip : tip.GetLocalizedString();
                }
#if UNITY_EDITOR
                if (tip.IsEmpty || string.IsNullOrEmpty(tip.TableReference)) return defaultTip;
                var collection =
                    UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(tip.TableReference);
                var entry = collection.SharedData.GetEntryFromReference(tip.TableEntryReference);
                return entry != null ? entry.Key : defaultTip;
#else
				return defaultTip;
#endif
            }
        }

        public List<QuestRequirement> Dependencies => dependencies;
        public List<ScriptableAction> ActionsOnTrack => actionsOnTrack;
        public List<ScriptableAction> ActionsOnComplete => actionsOnComplete;
        public Condition Condition
        {
            get => condition;
            set => condition = value;
        }
        
        public Condition FailCondition
        {
            get => failCondition;
            set => failCondition = value;
        }
        
        public Sprite Depiction
        {
            get 
            { 
                if (depiction) return depiction;
                return condition ? condition.Depiction : null;
            }
        }
        
        private SavedInt _state;

        public StateType State
        {
            get => (StateType) _state.Value;
            set
            {
                if (_state.Value == (int) value) return;
                _state.Value = (int) value;
            }
        }

        private List<QuestRequirement> ActiveDependencies => dependencies.FindAll(test => test && test.State != StateType.Completed);

        protected virtual string Directory => Path.Combine("Quests", "Requirements", ID);

        protected virtual void OnEnable()
        {
            LoadSavedData();
            BroadcastSavedData();
            foreach (QuestRequirement dependency in dependencies)
            {
                if (dependency) dependency.OnStateChanged += StartTracking;
            }
        }

        protected virtual void OnDisable()
        {
            foreach (QuestRequirement dependency in dependencies)
            {
                if (dependency) dependency.OnStateChanged -= StartTracking;
            }
        }

        protected void LoadSavedData()
        {
            _state = new SavedInt(Directory, "State.txt", 0, () =>
            {
                OnStateChanged?.Invoke();
                OnAnyStateChanged?.Invoke(this); 
            });
        }
        
        protected void BroadcastSavedData()
        {
            OnStateChanged?.Invoke();
            OnAnyStateChanged?.Invoke(this);
        }

        public void StartTracking()
        {
            if (condition) condition.OnConditionStateChanged -= OnCompletionConditionStateChanged;
            if (failCondition) failCondition.OnConditionStateChanged -= OnFailConditionStateChanged;
            if (ActiveDependencies.Count > 0 || State is StateType.Completed or StateType.Failed) return;
            State = StateType.Tracking;
            foreach (ScriptableAction action in actionsOnTrack)
                if (action) action.Invoke();
            if (condition && condition.Evaluate()) Complete();
            else if ( failCondition && failCondition.Evaluate()) Fail();
            else
            {
                if (condition) condition.OnConditionStateChanged += OnCompletionConditionStateChanged;
                if (failCondition) failCondition.OnConditionStateChanged += OnFailConditionStateChanged;
            }
        }
        
        private void OnCompletionConditionStateChanged()
        {
            if (condition && condition.Evaluate()) Complete();
        }
        
        private void OnFailConditionStateChanged()
        {
            if (failCondition && failCondition.Evaluate()) Fail();
        }

        [ContextMenu("Complete")]
        protected void Complete()
        {
            if (condition) condition.OnConditionStateChanged -= OnCompletionConditionStateChanged;
            if (failCondition) failCondition.OnConditionStateChanged -= OnFailConditionStateChanged;
            if (State == StateType.Completed) return;
            State = StateType.Completed;
            foreach (ScriptableAction action in actionsOnComplete)
                if (action) action.Invoke();
        }
        
        [ContextMenu("Fail")]
        protected void Fail()
        {
            if (condition) condition.OnConditionStateChanged -= OnCompletionConditionStateChanged;
            if (failCondition) failCondition.OnConditionStateChanged -= OnFailConditionStateChanged;
            if (State == StateType.Failed) return;
            State = StateType.Failed;
        }
        
#if UNITY_EDITOR

        public virtual void Reset()
        {
            GenerateNewID();
            quest = QuestsManager.Quests.Find(test => test.Requirements.Contains(this));
        }
        
        [ContextMenu("Generate New ID")]
        private void GenerateNewID()
        {
            ID = GUID.Generate().ToString();
        }
        
        public virtual void OnValidate()
        {
            PruneDependencies();
            PruneOnTrackActions();
            PruneOnCompleteActions();
            if (quest && EditorUtility.IsDirty(this))
            {
                EditorUtility.SetDirty(quest);
            }
        }

        [ContextMenu("Prune Dependencies")]
        private void PruneDependencies()
        {
            dependencies = dependencies.Distinct().ToList();
            dependencies.RemoveAll(dependency => !dependency || dependency.Quest != quest);
            dependencies.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        }
        
        [ContextMenu("Prune On Track Actions")]
        private void PruneOnTrackActions()
        {
            if (actionsOnTrack == null || actionsOnTrack.Count == 0) return;
            actionsOnTrack = actionsOnTrack.Distinct().ToList();
            actionsOnTrack.RemoveAll(action => !action);
        }
        
        [ContextMenu("Prune On Complete Actions")]
        private void PruneOnCompleteActions()
        {
            if (actionsOnComplete == null || actionsOnComplete.Count == 0) return;
            actionsOnComplete = actionsOnComplete.Distinct().ToList();
            actionsOnComplete.RemoveAll(action => !action);
        }

        [ContextMenu("Copy Name")]
        private void CopyName()
        {
            EditorGUIUtility.systemCopyBuffer = Name; 
        }
        
        public bool DescriptionHasErrors =>
            string.IsNullOrWhiteSpace(defaultDetail) &&
            (detail.IsEmpty || string.IsNullOrEmpty(detail.TableReference))
            || string.IsNullOrWhiteSpace(defaultTip) &&
            (tip.IsEmpty || string.IsNullOrEmpty(tip.TableReference));

        public bool DependenciesHaveErrors => dependencies.Contains(null);

        public bool CompletionConditionHasErrors => !condition || condition.HasErrors();
        
        public virtual bool HasErrors()
        {
            return string.IsNullOrWhiteSpace(ID)
                   || DescriptionHasErrors
                   || DependenciesHaveErrors
                   || CompletionConditionHasErrors;
        }

#endif
    }
}
