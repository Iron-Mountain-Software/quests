using ARISE.Gameplay.Quests.Extraction;
using IronMountain.Conditions;
using IronMountain.Conditions.Runtime;
using UnityEngine;

namespace ARISE.Gameplay.Quests.Conditions
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Gameplay/Quests/Conditions/Quest State")]
    public class ConditionQuestState : Condition
    {
        [SerializeField] private Quest quest;
        [SerializeField] private BooleanComparisonType comparisonType = BooleanComparisonType.Is;
        [SerializeField] private Quest.StateType state = Quest.StateType.None;

        private void OnEnable()
        {
            if (quest) quest.OnStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (quest) quest.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged() => FireOnConditionStateChanged();

        public override bool Evaluate() => quest && EvaluationUtilities.Compare((int) quest.State, (int) state, comparisonType);
        
        public override string DefaultName => comparisonType == BooleanComparisonType.Is
            ? (quest ? quest.name : "Null") + " is " + state
            : (quest ? quest.name : "Null") + " is NOT " + state;
        
        public override string NegatedName => comparisonType == BooleanComparisonType.Is
            ? (quest ? quest.name : "Null") + " is NOT " + state
            : (quest ? quest.name : "Null") + " is " + state;
        
        public override Sprite Depiction => null;
        
        public override bool HasErrors() => !quest;
    }
}