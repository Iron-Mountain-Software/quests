using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronMountain.Conditions;
using IronMountain.SaveSystem;
using IronMountain.ScriptableActions;
using UnityEngine;
using UnityEngine.Localization;

namespace ARISE.Gameplay.Quests.Extraction
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Gameplay/Quests/Quest")]
    public class Quest : ScriptableObject
    {
        [Serializable]
        public enum StateType
        {
            None = 0,
            Active = 1,
            Completed = 2
        }
        
        [Serializable]
        public enum StoryType
        {
            Main,
            Side
        }
        
        public static event Action<Quest> OnAnyStateChanged;
        public static event Action<Quest> OnAnyViewsChanged;

        public event Action OnStateChanged;
        public event Action OnViewsChanged;
        
        [SerializeField] private string id;
        [SerializeField] private LocalizedString localizedName;
        [SerializeField] private LocalizedString description;
        [SerializeField] private LocalizedString conclusion;

        [SerializeField] private StoryType type;
        [SerializeField] private int priority;

        [SerializeField] private Condition prerequisites;
        [SerializeField] private List<ScriptableAction> actionsOnActivate = new();
        [SerializeField] private List<ScriptableAction> actionsOnComplete = new();
        [SerializeField] private List<QuestRequirement> requirements = new();
        
        private SavedInt _state;
        private SavedInt _views;

        public string ID
        {
            get => id;
            set => id = value;
        }

        public virtual string Name => name;
        public int Priority => priority;
        public StoryType Type => type;

        public Condition Prerequisites
        {
            get => prerequisites;
            set => prerequisites = value;
        }

        public List<ScriptableAction> ActionsOnActivate => actionsOnActivate;
        public List<ScriptableAction> ActionsOnComplete => actionsOnComplete;
        public List<QuestRequirement> Requirements => requirements;
        public List<QuestRequirement> CompletedRequirements =>
            Requirements.FindAll(requirement => requirement.State == QuestRequirement.StateType.Completed);

        public bool ReadyToComplete => State == StateType.Active && CompletedRequirements.Count == Requirements.Count;

        public string LocalizedName
        {
            get
            {
                if (Application.isPlaying)
                {
                    return localizedName.IsEmpty ? string.Empty : localizedName.GetLocalizedString();
                }
#if UNITY_EDITOR
                if (localizedName.IsEmpty || string.IsNullOrEmpty(localizedName.TableReference)) return string.Empty;
                var collection =
                    UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(localizedName
                        .TableReference);
                var entry = collection.SharedData.GetEntryFromReference(localizedName.TableEntryReference);
                return entry != null ? entry.Key : string.Empty;
#else
				return string.Empty;
#endif
            }
        }

        public string Description
        {
            get
            {
                if (Application.isPlaying)
                {
                    return description.IsEmpty ? string.Empty : description.GetLocalizedString();
                }
#if UNITY_EDITOR
                if (description.IsEmpty || string.IsNullOrEmpty(description.TableReference)) return string.Empty;
                var collection =
                    UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(description
                        .TableReference);
                var entry = collection.SharedData.GetEntryFromReference(description.TableEntryReference);
                return entry != null ? entry.Key : string.Empty;
#else
				return string.Empty;
#endif
            }
        }

        public string Conclusion
        {
            get
            {
                if (Application.isPlaying)
                {
                    return conclusion.IsEmpty ? string.Empty : conclusion.GetLocalizedString();
                }
#if UNITY_EDITOR
                if (conclusion.IsEmpty || string.IsNullOrEmpty(conclusion.TableReference)) return string.Empty;
                var collection =
                    UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(conclusion
                        .TableReference);
                var entry = collection.SharedData.GetEntryFromReference(conclusion.TableEntryReference);
                return entry != null ? entry.Key : string.Empty;
#else
				return string.Empty;
#endif
            }
        }

        public StateType State
        {
            get => (StateType) _state.Value;
            private set
            {
                if (_state.Value == (int) value) return;
                _state.Value = (int) value;
            }
        }

        public int Views
        {
            get => _views.Value;
            set
            {
                if (_views.Value == value) return;
                _views.Value = value;
            }
        }
        
        protected virtual string Directory => Path.Combine("Quests", ID);

        protected virtual void OnEnable()
        {
            LoadSavedInformation();
            BroadcastSavedInformation();
            if (prerequisites != null) prerequisites.OnConditionStateChanged += OnPrerequisitesStateChanged;
        }

        protected virtual void OnDisable()
        {
            if (prerequisites != null) prerequisites.OnConditionStateChanged -= OnPrerequisitesStateChanged;
        }

        protected void LoadSavedInformation()
        {
            string directory = Directory;
            _state = new SavedInt(directory, "State.txt", 0, () =>
            {
                OnStateChanged?.Invoke();
                OnAnyStateChanged?.Invoke(this);
            });
            _views = new SavedInt(directory, "Views.txt", 0, () =>
            {
                OnViewsChanged?.Invoke();
                OnAnyViewsChanged?.Invoke(this);
            });
        }

        protected void BroadcastSavedInformation()
        {
            OnStateChanged?.Invoke();
            OnAnyStateChanged?.Invoke(this);
            OnViewsChanged?.Invoke();
            OnAnyViewsChanged?.Invoke(this);
        }

        public void Refresh()
        {
            switch (State)
            {
                case StateType.None:
                    if (!prerequisites || prerequisites.Evaluate()) Activate();
                    break;
                case StateType.Active:
                    foreach (QuestRequirement requirement in requirements)
                    {
                        if (requirement) requirement.StartTracking();
                    }
                    break;
                case StateType.Completed:
                    break;
            }
        }

        private void OnPrerequisitesStateChanged()
        {
            if (State != StateType.None) return;
            if (!prerequisites || prerequisites.Evaluate()) Activate();
        }
        
        [ContextMenu("Activate", false, 0)]
        public virtual bool Activate()
        {
            if (State != StateType.None) return false;
            State = StateType.Active;
            foreach (ScriptableAction action in actionsOnActivate)
                if (action) action.Invoke();
            foreach (QuestRequirement requirement in Requirements) requirement.StartTracking();
            return true;
        }
        
        [ContextMenu("Complete", false, 0)]
        public virtual bool Complete()
        {
            if (State != StateType.Active) return false;
            State = StateType.Completed;
            foreach (ScriptableAction action in actionsOnComplete)
                if (action) action.Invoke();
            return true;
        }

#if UNITY_EDITOR
        
        public virtual void Reset()
        {
            GenerateNewID();
            PruneRequirements();
            RenameComponents();
        }

        public virtual void OnValidate()
        {
            PruneRequirements();
            RenameComponents();
        }
        
        [ContextMenu("Generate New ID")]
        private void GenerateNewID()
        {
            ID = UnityEditor.GUID.Generate().ToString();
        }

        [ContextMenu("Prune Requirements")]
        private void PruneRequirements()
        {
            requirements = requirements.Distinct().ToList();
            requirements.RemoveAll(requirement => !requirement || requirement.Quest != this);
        }

        [ContextMenu("Rename Components")]
        private void RenameComponents()
        {
            int integerPrefix = 0, decimalPrefix = 0;
            if (prerequisites)
            {
                prerequisites.name = "0.0 ─ Prerequisites - " + prerequisites.DefaultName;
            }
            for (int i = 0; i < Requirements.Count; i++)
            {
                QuestRequirement requirement = Requirements[i];
                if (!requirement) continue;
                string prefix = (i + 1).ToString();
                bool noCondition = !requirement.Condition;
                bool noActions = requirement.ActionsOnTrack.Count == 0
                                  && requirement.ActionsOnComplete.Count == 0;
                requirement.name = prefix + (noCondition && noActions ? ".0 ─ " : ".0 ┬ ") + "Requirement " + prefix;
                decimalPrefix = 1;
                if (requirement.Condition)
                {
                    decimalPrefix++;
                    requirement.Condition.name = prefix + "." + decimalPrefix + (noActions ? " └─ " : " ├─ ") + " Condition - " + requirement.Condition.DefaultName;
                }
                for (int onTrackIndex = 0; onTrackIndex < requirement.ActionsOnTrack.Count; onTrackIndex++)
                {
                    ScriptableAction action = requirement.ActionsOnTrack[onTrackIndex];
                    if (!action) continue;
                    bool isLast = onTrackIndex == requirement.ActionsOnTrack.Count - 1 && requirement.ActionsOnComplete.Count == 0;
                    decimalPrefix++;
                    action.name = prefix + "." + decimalPrefix + (isLast ? " └─ " : " ├─ ") + " On Track - " + action.ToString();
                }
                for (int onCompleteIndex = 0; onCompleteIndex < Requirements[i].ActionsOnComplete.Count; onCompleteIndex++)
                {
                    ScriptableAction action = requirement.ActionsOnComplete[onCompleteIndex];
                    if (!action) continue;
                    bool isLast = onCompleteIndex == requirement.ActionsOnComplete.Count - 1;
                    decimalPrefix++;
                    action.name = prefix + "." + decimalPrefix + (isLast ? " └─ " : " ├─ ") + " On Complete - "  + action;
                }
            }

            integerPrefix = requirements.Count + 1;
            decimalPrefix = 0;
            foreach (ScriptableAction action in actionsOnActivate)
            {
                if (!action) continue;
                action.name = integerPrefix + "." + decimalPrefix + " ─ On Start - " + action;
                decimalPrefix++;
            }
            foreach (ScriptableAction action in actionsOnComplete)
            {
                if (!action) continue;
                action.name = integerPrefix + "." + decimalPrefix + " On Complete - " + action;
                decimalPrefix++;
            }
        }
        
        public virtual bool HasErrors()
        {
            foreach (QuestRequirement questRequirement in Requirements)
            {
                if (!questRequirement) return true;
                if (questRequirement.HasErrors()) return true;
            }

            return string.IsNullOrWhiteSpace(ID)
                   || localizedName.IsEmpty || string.IsNullOrEmpty(localizedName.TableReference)
                   || description.IsEmpty || string.IsNullOrEmpty(description.TableReference)
                   || conclusion.IsEmpty || string.IsNullOrEmpty(conclusion.TableReference);
        }

#endif
    }
}
