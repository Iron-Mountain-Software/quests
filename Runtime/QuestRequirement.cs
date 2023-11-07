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
            Completed = 2
        }
        
        [SerializeField] private string id;
        
        public string ID
        {
            get => id;
            set => id = value;
        }

        public virtual string Name => condition ? condition.DefaultName : "NULL";
    
        public static event Action<QuestRequirement> OnAnyStateChanged;
        
        public event Action OnStateChanged;
        
        [SerializeField] private Quest quest;
        [SerializeField] private LocalizedString detail;
        [SerializeField] private LocalizedString tip;
        [SerializeField] private List<QuestRequirement> dependencies = new ();
        [SerializeField] private List<ScriptableAction> actionsOnTrack = new ();
        [SerializeField] private List<ScriptableAction> actionsOnComplete = new ();
        [SerializeField] public Condition condition;
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
                    return detail.IsEmpty ? string.Empty : detail.GetLocalizedString();
                }
#if UNITY_EDITOR
                if (detail.IsEmpty || string.IsNullOrEmpty(detail.TableReference)) return string.Empty;
                var collection =
                    UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(detail.TableReference);
                var entry = collection.SharedData.GetEntryFromReference(detail.TableEntryReference);
                return entry != null ? entry.Key : string.Empty;
#else
				return string.Empty;
#endif
            }
        }

        public string Tip
        {
            get
            {
                if (Application.isPlaying)
                {
                    return tip.IsEmpty ? string.Empty : tip.GetLocalizedString();
                }
#if UNITY_EDITOR
                if (tip.IsEmpty || string.IsNullOrEmpty(tip.TableReference)) return string.Empty;
                var collection =
                    UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(tip.TableReference);
                var entry = collection.SharedData.GetEntryFromReference(tip.TableEntryReference);
                return entry != null ? entry.Key : string.Empty;
#else
				return string.Empty;
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

        private void OnConditionStateChanged()
        {
            if (condition.Evaluate()) Complete();
        }

        public void StartTracking()
        {
            if (condition) condition.OnConditionStateChanged -= OnConditionStateChanged;
            if (ActiveDependencies.Count > 0 || State == StateType.Completed) return;
            if (State != StateType.Tracking)
            {
                State = StateType.Tracking;
                foreach (ScriptableAction action in actionsOnTrack)
                    if (action) action.Invoke();
            }
            if (condition)
            {
                if (condition.Evaluate()) Complete();
                else condition.OnConditionStateChanged += OnConditionStateChanged;
            }
        }

        [ContextMenu("Complete")]
        protected void Complete()
        {
            if (condition) condition.OnConditionStateChanged -= OnConditionStateChanged;
            if (State == StateType.Completed) return;
            State = StateType.Completed;
            foreach (ScriptableAction action in actionsOnComplete)
                if (action) action.Invoke();
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

        public virtual bool HasErrors()
        {
            return string.IsNullOrWhiteSpace(ID)
                   || detail.IsEmpty || string.IsNullOrEmpty(detail.TableReference)
                   || tip.IsEmpty || string.IsNullOrEmpty(tip.TableReference)
                   || dependencies.Contains(null)
                   || !condition;
        }

#endif
    }
}
