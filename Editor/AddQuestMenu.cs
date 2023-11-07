using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    public static class AddQuestMenu
    {
        private static readonly List<Type> QuestTypes;

        static AddQuestMenu()
        {
            QuestTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && (type == typeof(Quest) || type.IsSubclassOf(typeof(Quest))))
                .ToList();
        }
        
        public static void Open()
        {
            if (QuestTypes.Count > 1)
            {
                GenericMenu menu = new GenericMenu();
                foreach (Type derivedType in QuestTypes)
                {
                    menu.AddItem(new GUIContent(
                            "Add " + derivedType.Name),
                            false,
                            () => Save(ScriptableObject.CreateInstance(derivedType) as Quest, "New " + derivedType.Name));
                }
                menu.ShowAsContext();
            }
            else Save(ScriptableObject.CreateInstance(typeof(Quest)) as Quest, "New Quest");
        }

        private static void Save(Quest quest, string name)
        {
            string scriptableObjectsFolder = Path.Combine("Assets", "Scriptable Objects");
            string questsFolder = Path.Combine(scriptableObjectsFolder, "Quests");
            if (!AssetDatabase.IsValidFolder(scriptableObjectsFolder)) AssetDatabase.CreateFolder("Assets", "Scriptable Objects");
            if (!AssetDatabase.IsValidFolder(questsFolder)) AssetDatabase.CreateFolder(scriptableObjectsFolder, "Quests");
            string path = Path.Combine(questsFolder, name + ".asset");
            AssetDatabase.CreateAsset(quest, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = quest;
        }
    }
}