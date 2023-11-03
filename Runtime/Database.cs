using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARISE.Gameplay.Quests.Extraction;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ARISE.Gameplay.Quests
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Gameplay/Quests/Database")]
    public class Database : ScriptableObject
    {
        [SerializeField] private List<Quest> quests = new ();

        private Dictionary<string, Quest> _dictionary;

        public List<Quest> Quests => quests;
        
        public Quest GetQuestByName(string sceneName)
        {
            return quests.Find(test=> test.name == sceneName);
        }
        
        public Quest GetQuestByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            if (_dictionary == null) RebuildDictionary();
            return _dictionary.ContainsKey(id) 
                ? _dictionary[id]
                : null;
        }

        public Quest GetRandomQuest()
        {
            if (_dictionary == null) RebuildDictionary();
            return _dictionary.Count > 0
                ? _dictionary.ElementAt(Random.Range(0, _dictionary.Count)).Value
                : null;
        }

        public void SortList()
        {
            quests.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));   
        }

        public void RebuildDictionary()
        {
            if (_dictionary != null) _dictionary.Clear();
            else _dictionary = new Dictionary<string, Quest>();
            int failures = 0;
            foreach (Quest quest in quests)
            {
                try  
                {
                    if (!quest) throw new Exception("Null Quest");
                    if (string.IsNullOrWhiteSpace(quest.ID)) throw new Exception("Quest with empty key: " + quest.Name);
                    if (_dictionary.ContainsKey(quest.ID)) throw new Exception("Quests with duplicate keys: " + quest.Name + ", " + _dictionary[quest.ID].Name);
                    _dictionary.Add(quest.ID, quest);
                }  
                catch (Exception exception)  
                {  
                    failures++;
                    if (quest) Debug.LogError(exception.Message, quest);
                    else Debug.LogError(exception.Message);
                }
            }
            Debug.Log("Rebuilt Dictionary: " + _dictionary.Count + " Successes, " + failures + " Failures");
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("<QUESTS>\n");
            foreach (Quest quest in quests)
            {
                result.Append(quest.ID);
                result.Append("\t");
                result.Append(quest.Name);
                result.Append("\n");
            }
            result.Append("</QUESTS>\n");
            return result.ToString();
        }
    }
}