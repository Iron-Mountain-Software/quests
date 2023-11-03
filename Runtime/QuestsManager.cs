using System.Collections.Generic;
using UnityEngine;

namespace IronMountain.Quests
{
    public class QuestsManager : MonoBehaviour
    {
        public static QuestsManager Instance { get; set; }
        
        public static Database Database => Instance ? Instance.database : null;
        public static List<Quest> Quests => Database ? Database.Quests : new List<Quest>();

        [SerializeField] private Database database;
        
        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
        }

        private void Start()
        {
            Refresh();
        }

        protected void Refresh()
        {
            if (!Database || Database.Quests == null) return;
            foreach (Quest quest in Database.Quests)
            {
                if (quest) quest.Refresh();
            }
        }
    }
}