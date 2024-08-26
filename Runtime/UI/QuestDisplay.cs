using System;
using UnityEngine;

namespace IronMountain.Quests.UI
{
    public class QuestDisplay : MonoBehaviour
    {
        public event Action OnQuestChanged;
        
        [SerializeField] private Quest quest;

        public Quest Quest
        {
            get => quest;
            set
            {
                if (quest == value) return;
                quest = value;
                OnQuestChanged?.Invoke();
            }
        }

        private void OnValidate() => OnQuestChanged?.Invoke();
    }
}
