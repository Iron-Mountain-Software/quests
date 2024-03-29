using IronMountain.Conditions;
using UnityEngine;
using UnityEngine.Serialization;

namespace IronMountain.Quests
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Gameplay/Quests/Conditions/Story Event State")]
    public class StoryEventStateCondition : Condition
    {
        [SerializeField] [FormerlySerializedAs("requirement")] private StoryEvent storyEvent;
        [SerializeField] private BooleanComparisonType comparisonType = BooleanComparisonType.Is;
        [SerializeField] private StoryEvent.StateType state = StoryEvent.StateType.Active;

        private void OnEnable()
        {
            if (storyEvent) storyEvent.OnStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (storyEvent) storyEvent.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged() => FireOnConditionStateChanged();

        public override bool Evaluate() => storyEvent && EvaluationUtilities.Compare((int) storyEvent.State, (int) state, comparisonType);
        
        public override Sprite Depiction => storyEvent is QuestRequirement questRequirement
            ? questRequirement.Depiction
            : null;
        
        public override bool HasErrors() => !storyEvent;

        public override string ToString() => comparisonType == BooleanComparisonType.Is
            ? (storyEvent ? storyEvent.name : "Null") + " is " + state
            : (storyEvent ? storyEvent.name : "Null") + " is NOT " + state;
    }
}