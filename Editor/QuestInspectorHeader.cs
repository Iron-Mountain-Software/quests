using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    public static class QuestInspectorHeader
    {
        public static QuestRequirement Draw(Quest quest, QuestRequirement selectedRequirement)
        {
            QuestRequirement newSelectedRequirement = selectedRequirement;
            GUILayout.Space(10);
            EditorGUI.BeginDisabledGroup(Selection.activeObject == quest);
            if (GUILayout.Button(quest.Name, GUILayout.ExpandWidth(true), GUILayout.MinHeight(30)))
            {
                Selection.activeObject = quest;
                newSelectedRequirement = null;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.BeginHorizontal();
            for (int index = 0; index < quest.Requirements.Count; index++)
            {
                QuestRequirement requirement = quest.Requirements[index];
                EditorGUI.BeginDisabledGroup(Selection.activeObject == requirement);
                if (GUILayout.Button((index + 1).ToString(), GUILayout.MinHeight(25)))
                {
                    Selection.activeObject = requirement;
                    newSelectedRequirement = requirement;
                }
                EditorGUI.EndDisabledGroup();
            }
            if (GUILayout.Button("+", GUILayout.MinHeight(25))) AddQuestRequirementMenu.Open(quest);
            EditorGUILayout.EndHorizontal();
            return newSelectedRequirement;
        }
    }
}
