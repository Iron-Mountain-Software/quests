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
            if (GUILayout.Button(quest.Name, GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                Selection.activeObject = quest;
                newSelectedRequirement = null;
                GUI.FocusControl(null);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.BeginHorizontal();
            if (quest && quest.Requirements.Count > 0)
            {
                for (int index = 0; index < quest.Requirements.Count; index++)
                {
                    QuestRequirement requirement = quest.Requirements[index];
                    EditorGUI.BeginDisabledGroup(Selection.activeObject == requirement);
                    if (GUILayout.Button((index + 1) + ". " + requirement.Detail, GUILayout.MinHeight(25)))
                    {
                        Selection.activeObject = requirement;
                        newSelectedRequirement = requirement;
                        GUI.FocusControl(null);
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No requirements to show.");
            }

            
            if (GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(25))) AddQuestRequirementMenu.Open(quest);
            EditorGUILayout.EndHorizontal();
            return newSelectedRequirement;
        }
    }
}
