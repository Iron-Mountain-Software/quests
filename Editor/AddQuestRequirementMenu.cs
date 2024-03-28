using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    public static class AddQuestRequirementMenu
    {
        private static readonly List<Type> QuestRequirementTypes;

        static AddQuestRequirementMenu()
        {
            QuestRequirementTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && (type == typeof(QuestRequirement) || type.IsSubclassOf(typeof(QuestRequirement))))
                .ToList();
        }
        
        public static void Open(Quest quest)
        {
            if (QuestRequirementTypes.Count == 1)
            {
                QuestRequirement requirement = ScriptableObject.CreateInstance<QuestRequirement>();
                QuestRequirementsEditor.AddRequirementToQuest(requirement, quest);
            }
            else
            {
                GenericMenu menu = new GenericMenu();
                foreach (Type derivedType in QuestRequirementTypes)
                {
                    menu.AddItem(new GUIContent("Add " + derivedType.Name), false,
                        () =>
                        {
                            QuestRequirement requirement = ScriptableObject.CreateInstance(derivedType) as QuestRequirement;
                            QuestRequirementsEditor.AddRequirementToQuest(requirement, quest);
                            QuestsEditorWindow.Open(quest, requirement);
                        });
                }
                menu.ShowAsContext();
            }
        }
    }
}