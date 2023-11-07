using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    public static class QuestRequirementsEditor
    {
        public static void AddRequirementToQuest(QuestRequirement requirement, Quest quest)
        {
            if (!requirement || !quest) return;
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(quest))) return;
            quest.OnValidate();
            AssetDatabase.AddObjectToAsset(requirement, quest);
            quest.Requirements.Add(requirement);
            requirement.Quest = quest;
            requirement.OnValidate();
            quest.OnValidate();
            Selection.activeObject = requirement;
            AssetDatabase.SaveAssets();
        }

        public static void RemoveRequirementFromQuest(QuestRequirement requirement)
        {
            if (!requirement || string.IsNullOrEmpty(AssetDatabase.GetAssetPath(requirement))) return;
            AssetDatabase.RemoveObjectFromAsset(requirement);
            Quest quest = requirement.Quest;
            if (quest) quest.Requirements.RemoveAll(test => test == requirement);
            requirement.Quest = null;
            Object.DestroyImmediate(requirement);
            if (quest)
            {
                quest.OnValidate();
                Selection.activeObject = quest;
            }
            AssetDatabase.SaveAssets();
        }
    }
}
