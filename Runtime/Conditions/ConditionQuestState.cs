using IronMountain.Conditions;
using UnityEngine;

namespace IronMountain.Quests.Conditions
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
        
        public override Sprite Depiction => null;
        
        public override bool HasErrors() => !quest;

        public override string ToString() => comparisonType == BooleanComparisonType.Is
            ? (quest ? quest.name : "Null") + " is " + state
            : (quest ? quest.name : "Null") + " is NOT " + state;
    }
}