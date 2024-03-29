using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

namespace IronMountain.Quests
{
    public class QuestRequirement : StoryEvent
    {
        public virtual string Name => completionCondition ? completionCondition.ToString() : "NULL";

        [SerializeField] private Quest quest;
        [SerializeField] private string defaultDetail;
        [SerializeField] private LocalizedString detail;
        [SerializeField] private string defaultTip;
        [SerializeField] private LocalizedString tip;
        [SerializeField] private Sprite depiction;
        
        protected override string Directory => Path.Combine("Quests", "Requirements", ID);

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

        public Sprite Depiction
        {
            get 
            { 
                if (depiction) return depiction;
                return completionCondition ? completionCondition.Depiction : null;
            }
        }

        protected override bool PrerequisitesSatisfied =>
            Quest && Quest.State == StateType.Active && base.PrerequisitesSatisfied;

        protected override bool CompletionConditionSatisfied =>
            Quest && Quest.State == StateType.Active && base.CompletionConditionSatisfied;

        protected override bool FailConditionSatisfied =>
            Quest && Quest.State == StateType.Active && base.FailConditionSatisfied;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            if (quest) quest.OnStateChanged += Refresh;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (quest) quest.OnStateChanged -= Refresh;
        }
        
        public override void Refresh()
        {
            StopListening();
            StateType myState = State;
            StateType questState = quest ? quest.State : StateType.Inactive;
            if (questState is StateType.Inactive)
            {
                Restart();
            }
            else if (questState is StateType.Active)
            {
                if (myState is StateType.Inactive) Restart();
                else if (myState is StateType.Active) Activate();
            }
            else if (myState is StateType.Active) Restart();
        }

#if UNITY_EDITOR

        public virtual void Reset()
        {
            GenerateNewID();
            quest = QuestsManager.Quests.Find(test => test.Requirements.Contains(this));
        }
        
        public override void OnValidate()
        {
            base.OnValidate();
            if (!quest) return;
            quest.OnValidate();
            if (EditorUtility.IsDirty(this)) EditorUtility.SetDirty(quest);
        }

        public bool DescriptionHasErrors =>
            string.IsNullOrWhiteSpace(defaultDetail) &&
            (detail.IsEmpty || string.IsNullOrEmpty(detail.TableReference))
            || string.IsNullOrWhiteSpace(defaultTip) &&
            (tip.IsEmpty || string.IsNullOrEmpty(tip.TableReference));
        
        public override bool HasErrors() =>
            base.HasErrors()
            || DescriptionHasErrors;
        
        public override string GetDocumentation()
        {
            StringBuilder documentation = new StringBuilder();
            documentation.AppendLine(Name);
            documentation.AppendLine("  ┣━ " + Detail);
            documentation.AppendLine("  ┗━ " + Tip);
            return documentation.ToString();
        }
#endif
    }
}
