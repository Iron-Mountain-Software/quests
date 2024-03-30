using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IronMountain.SaveSystem;
using IronMountain.ScriptableActions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

namespace IronMountain.Quests
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Gameplay/Quests/Quest")]
    public class Quest : StoryEvent
    {
        [Serializable]
        public enum StoryType
        {
            Main,
            Side
        }

        public event Action OnIsListeningChanged;
        public static event Action<Quest> OnAnyViewsChanged;
        public event Action OnViewsChanged;
        
        [SerializeField] private string defaultName;
        [SerializeField] private LocalizedString localizedName;
        [SerializeField] private string defaultDescription;
        [SerializeField] private LocalizedString description;
        [SerializeField] private string defaultConclusion;
        [SerializeField] private LocalizedString conclusion;

        [SerializeField] private StoryType type;
        [SerializeField] private int priority;
        [SerializeField] private bool universal = true;
        [SerializeField] private List<SceneAsset> sceneAssets;
        [SerializeField] private List<string> sceneNames;

        [SerializeField] private List<QuestRequirement> requirements = new();
        
        private SavedInt _views;
        private bool _isListening;
        
        public int Priority
        {
            get => priority;
            set => priority = value;
        }

        public StoryType Type
        {
            get => type;
            set => type = value;
        }

        public List<QuestRequirement> Requirements => requirements;
        
        public string Name
        {
            get
            {
                if (Application.isPlaying)
                {
                    return localizedName.IsEmpty ? defaultName : localizedName.GetLocalizedString();
                }
#if UNITY_EDITOR
                if (localizedName.IsEmpty || string.IsNullOrEmpty(localizedName.TableReference)) return defaultName;
                var collection =
                    UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(localizedName
                        .TableReference);
                var entry = collection.SharedData.GetEntryFromReference(localizedName.TableEntryReference);
                return entry != null ? entry.Key : defaultName;
#else
				return defaultName;
#endif
            }
            set => defaultName = value;
        }

        public string Description
        {
            get
            {
                if (Application.isPlaying)
                {
                    return description.IsEmpty ? defaultDescription : description.GetLocalizedString();
                }
#if UNITY_EDITOR
                if (description.IsEmpty || string.IsNullOrEmpty(description.TableReference)) return defaultDescription;
                var collection =
                    UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(description
                        .TableReference);
                var entry = collection.SharedData.GetEntryFromReference(description.TableEntryReference);
                return entry != null ? entry.Key : defaultDescription;
#else
				return defaultDescription;
#endif
            }
            set => defaultDescription = value;
        }

        public string Conclusion
        {
            get
            {
                if (Application.isPlaying)
                {
                    return conclusion.IsEmpty ? defaultConclusion : conclusion.GetLocalizedString();
                }
#if UNITY_EDITOR
                if (conclusion.IsEmpty || string.IsNullOrEmpty(conclusion.TableReference)) return defaultConclusion;
                var collection =
                    UnityEditor.Localization.LocalizationEditorSettings.GetStringTableCollection(conclusion
                        .TableReference);
                var entry = collection.SharedData.GetEntryFromReference(conclusion.TableEntryReference);
                return entry != null ? entry.Key : defaultConclusion;
#else
				return defaultConclusion;
#endif
            }
            set => defaultConclusion = value;
        }

        public bool IsListening
        {
            get => _isListening;
            set
            {
                if (_isListening == value) return;
                _isListening = value;
                OnIsListeningChanged?.Invoke();
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
        
        protected override bool PrerequisitesSatisfied =>
            IsListening && base.PrerequisitesSatisfied;

        protected override bool CompletionConditionSatisfied =>
            IsListening && base.CompletionConditionSatisfied;

        protected override bool FailConditionSatisfied =>
            IsListening && base.FailConditionSatisfied;
        
        protected override string Directory => Path.Combine("Quests", ID);

        protected override void OnEnable()
        {            
            RefreshIsListeningState();
            base.OnEnable();
            QuestsManager.Register(this);
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnloaded;
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            QuestsManager.Unregister(this);
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }
        
        private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode) => RefreshIsListeningState();
        private void SceneUnloaded(Scene scene) => RefreshIsListeningState();

        private void RefreshIsListeningState()
        {
            bool shouldListen = false;
            if (universal) shouldListen = true;
            else
            {
                for (int i = 0; i < SceneManager.loadedSceneCount; i++)
                {
                    if (!sceneNames.Contains(SceneManager.GetSceneAt(i).name)) continue;
                    shouldListen = true;
                    break;
                }
            }
            IsListening = shouldListen;
        }
        
        protected override void LoadSavedData()
        {
            base.LoadSavedData();
            _views = new SavedInt(Directory, "Views.txt", 0, () =>
            {
                OnViewsChanged?.Invoke();
                OnAnyViewsChanged?.Invoke(this);
            });
        }

        protected override void BroadcastSavedData()
        {
            base.BroadcastSavedData();
            OnViewsChanged?.Invoke();
            OnAnyViewsChanged?.Invoke(this);
        }

        public override void Refresh()
        {
            StopListening();
            RefreshIsListeningState();
            StateType state = State;
            if (state is StateType.Inactive) Restart();
            else if (state is StateType.Active) Activate();
            foreach (QuestRequirement requirement in requirements)
            {
                if (requirement) requirement.Refresh();
            }
        }
        
        public override void Restart()
        {
            base.Restart();
            foreach (QuestRequirement requirement in requirements)
            {
                if (requirement) requirement.Restart();
            }
        }

#if UNITY_EDITOR
        
        public virtual void Reset()
        {
            GenerateNewID();
            PruneRequirements();
            RenameComponents();
        }

        public override void OnValidate()
        {
            base.OnValidate();
            RefreshScenes();
            PruneRequirements();
            RenameComponents();
        }

        private void RefreshScenes()
        {
            sceneNames.Clear();
            if (universal) sceneAssets.Clear();
            foreach (SceneAsset sceneAsset in sceneAssets)
            {
                if (!sceneAsset) continue;
                sceneNames.Add(sceneAsset.name);
            }
        }

        [ContextMenu("Prune Requirements")]
        private void PruneRequirements()
        {
            if (requirements == null || requirements.Count == 0) return;
            requirements = requirements.Distinct().ToList();
            requirements.RemoveAll(requirement => !requirement || requirement.Quest != this);
        }

        [ContextMenu("Rename Components")]
        private void RenameComponents()
        {
            int integerPrefix = 0, decimalPrefix = 0;
            
            if (prerequisites)
            {
                prerequisites.name = integerPrefix + "." + decimalPrefix + " ─ Prerequisites - " + prerequisites;
                decimalPrefix++;
            }
            
            foreach (ScriptableAction action in actionsOnActivate)
            {
                if (!action) continue;
                action.name = integerPrefix + "." + decimalPrefix + " ─ On Activate - " + action;
                decimalPrefix++;
            }
            
            if (failCondition)
            {
                failCondition.name = integerPrefix + "." + decimalPrefix + " ─ To Fail - " + failCondition;
                decimalPrefix++;
            }
            
            foreach (ScriptableAction action in actionsOnFail)
            {
                if (!action) continue;
                action.name = integerPrefix + "." + decimalPrefix + " ─ On Fail - " + action;
                decimalPrefix++;
            }
            
            if (completionCondition)
            {
                completionCondition.name = integerPrefix + "." + decimalPrefix + " ─ To Complete - " + completionCondition;
                decimalPrefix++;
            }
            
            foreach (ScriptableAction action in actionsOnComplete)
            {
                if (!action) continue;
                action.name = integerPrefix + "." + decimalPrefix + " ─ On Complete - " + action;
                decimalPrefix++;
            }

            for (int i = 0; i < Requirements.Count; i++)
            {
                QuestRequirement requirement = Requirements[i];
                if (!requirement) continue;
                string prefix = (i + 1).ToString();
                bool noCondition = !requirement.completionCondition;
                bool noActions = requirement.actionsOnActivate.Count == 0
                                  && requirement.actionsOnFail.Count == 0
                                  && requirement.actionsOnComplete.Count == 0;
                requirement.name = prefix + (noCondition && noActions ? ".0 ─ " : ".0 ┬ ") + requirement.Detail;
                decimalPrefix = 1;
                
                if (requirement.prerequisites)
                {
                    requirement.prerequisites.name = prefix + "." + decimalPrefix + (noActions ? " └─ " : " ├─ ") + " Prerequisites - " + requirement.prerequisites;
                    decimalPrefix++;
                }
                
                for (int onTrackIndex = 0; onTrackIndex < requirement.actionsOnActivate.Count; onTrackIndex++)
                {
                    ScriptableAction action = requirement.actionsOnActivate[onTrackIndex];
                    if (!action) continue;
                    bool isLast = onTrackIndex == requirement.actionsOnActivate.Count - 1 && requirement.actionsOnComplete.Count == 0;
                    action.name = prefix + "." + decimalPrefix + (isLast ? " └─ " : " ├─ ") + " On Activate - " + action;
                    decimalPrefix++;
                }
                
                if (requirement.failCondition)
                {
                    bool isLast = requirement.actionsOnFail is null or {Count: 0}
                                  && !requirement.completionCondition
                                  && requirement.actionsOnComplete is null or {Count: 0};
                    requirement.failCondition.name = prefix + "." + decimalPrefix + (isLast ? " └─ " : " ├─ ") + " To Fail - " + requirement.failCondition;
                    decimalPrefix++;
                }

                for (int onFailIndex = 0; onFailIndex < Requirements[i].actionsOnFail.Count; onFailIndex++)
                {
                    ScriptableAction action = requirement.actionsOnFail[onFailIndex];
                    if (!action) continue;
                    bool isLast = onFailIndex == requirement.actionsOnFail.Count - 1
                        && !requirement.completionCondition
                        && requirement.actionsOnComplete is null or {Count: 0};
                    action.name = prefix + "." + decimalPrefix + (isLast ? " └─ " : " ├─ ") + " On Fail - "  + action;
                    decimalPrefix++;
                }
                
                if (requirement.completionCondition)
                {
                    bool isLast = requirement.actionsOnComplete is null or {Count: 0};
                    requirement.completionCondition.name = prefix + "." + decimalPrefix + (isLast ? " └─ " : " ├─ ") + " To Complete - " + requirement.completionCondition;
                    decimalPrefix++;
                }
                
                for (int onCompleteIndex = 0; onCompleteIndex < requirement.actionsOnComplete.Count; onCompleteIndex++)
                {
                    ScriptableAction action = requirement.actionsOnComplete[onCompleteIndex];
                    if (!action) continue;
                    bool isLast = onCompleteIndex == requirement.actionsOnComplete.Count - 1;
                    action.name = prefix + "." + decimalPrefix + (isLast ? " └─ " : " ├─ ") + " On Complete - "  + action;
                    decimalPrefix++;
                }
            }
        }

        public override string GetDocumentation()
        {
            StringBuilder documentation = new StringBuilder();
            documentation.AppendLine("QUEST:       " + name);
            documentation.AppendLine("PRIORITY:    " + priority);
            documentation.AppendLine("NAME:        " + Name);
            documentation.AppendLine("DESCRIPTION: " + Description);
            documentation.AppendLine("REQUIREMENTS:" + Description);
            int index = 1;
            foreach (QuestRequirement requirement in requirements)
            {
                if (!requirement) continue;
                documentation.AppendLine(index + ") " + requirement.GetDocumentation());
                index++;
            }
            documentation.AppendLine("CONCLUSION: " + Conclusion);
            return documentation.ToString();
        }

        public bool DescriptionHasErrors =>
            string.IsNullOrWhiteSpace(defaultName) &&
            (localizedName.IsEmpty || string.IsNullOrEmpty(localizedName.TableReference))
            || string.IsNullOrWhiteSpace(defaultDescription) &&
            (description.IsEmpty || string.IsNullOrEmpty(description.TableReference))
            || string.IsNullOrWhiteSpace(defaultConclusion) &&
            (conclusion.IsEmpty || string.IsNullOrEmpty(conclusion.TableReference));
        
        public bool RequirementsHaveErrors
        {
            get
            {
                foreach (QuestRequirement questRequirement in Requirements)
                {
                    if (!questRequirement || questRequirement.HasErrors()) return true;
                }
                return false;
            }
        }

        public override bool HasErrors() =>
            base.HasErrors()
            || DescriptionHasErrors
            || RequirementsHaveErrors;
#endif
    }
}
