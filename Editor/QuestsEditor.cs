using System.IO;
using IronMountain.Conditions;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    public static class QuestsEditor
    {
        private static readonly string QuestsPath = "Assets/Scriptable Objects/Quests/";
        
        public static void CreateNewQuest(string questName)
        {
            Quest quest = ScriptableObject.CreateInstance(typeof(Quest)) as Quest;
            string path = Path.Combine(QuestsPath, questName + ".asset");
            AssetDatabase.CreateAsset(quest, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = quest;
        }

        public static void DeleteQuest(Quest quest)
        {
            if (!quest) return;
            string path = AssetDatabase.GetAssetPath(quest);
            if (!string.IsNullOrEmpty(path)) AssetDatabase.DeleteAsset(path);
        }

        public static void RemovePrerequisitesFromQuest(Quest quest)
        {
            if (!quest) return;
            Condition prerequisites = quest.Prerequisites;
            if (!prerequisites || string.IsNullOrEmpty(AssetDatabase.GetAssetPath(prerequisites))) return;
            AssetDatabase.RemoveObjectFromAsset(prerequisites);
            quest.Prerequisites = null;
            Object.DestroyImmediate(prerequisites);
            if (quest) quest.OnValidate();
            AssetDatabase.SaveAssets();
        }
    }
}