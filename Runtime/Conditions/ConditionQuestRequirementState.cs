using IronMountain.Conditions;
using UnityEngine;

namespace IronMountain.Quests.Conditions
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Gameplay/Quests/Conditions/Quest Requirement Tracking")]
    public class ConditionQuestRequirementState : Condition
    {
        [SerializeField] private QuestRequirement requirement;
        [SerializeField] private BooleanComparisonType comparisonType = BooleanComparisonType.Is;
        [SerializeField] private QuestRequirement.StateType state = QuestRequirement.StateType.Active;

        private void OnEnable()
        {
            if (requirement) requirement.OnStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (requirement) requirement.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged() => FireOnConditionStateChanged();

        public override bool Evaluate() => requirement && EvaluationUtilities.Compare((int) requirement.State, (int) state, comparisonType);
        
        public override Sprite Depiction => requirement ? requirement.Depiction : null;
        
        public override bool HasErrors() => !requirement;

        public override string ToString() => comparisonType == BooleanComparisonType.Is
            ? (requirement ? requirement.name : "Null") + " is " + state
            : (requirement ? requirement.name : "Null") + " is NOT " + state;
    }
}