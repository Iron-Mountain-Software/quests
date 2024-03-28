using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor.Windows
{
    public class NewQuestWindow : EditorWindow
    {
        private static int _questTypeIndex = 0;

        private string _folder = Path.Join("Assets", "Scriptable Objects", "Quests");
        private string _name = "New Quest";
        private string _description = string.Empty;
        private string _conclusion = string.Empty;
        private Quest.StoryType _type = Quest.StoryType.Main;
        private int _priority = 0;

        public static void Open()
        {
            NewQuestWindow window = GetWindow(typeof(NewQuestWindow), false, "Create Quest", true) as NewQuestWindow;
            window.minSize = new Vector2(520, 225);
            window.maxSize = new Vector2(520, 225);
            window.wantsMouseMove = true;
        }

        protected void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            _folder = EditorGUILayout.TextField("Folder: ", _folder);
            if (GUILayout.Button("Current", GUILayout.Width(60))) _folder = GetCurrentFolder();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            _name = EditorGUILayout.TextField("Name", _name);
            if (GUILayout.Button("Current", GUILayout.Width(60))) _name = GetCurrentName();
            EditorGUILayout.EndHorizontal();

            _questTypeIndex = EditorGUILayout.Popup("Type", _questTypeIndex, TypeIndex.QuestTypeNames);
            
            EditorGUILayout.Space(10);
            
            _description = EditorGUILayout.TextField("Description:", _description);
            _conclusion = EditorGUILayout.TextField("Conclusion:", _conclusion);
            _type = (Quest.StoryType) EditorGUILayout.EnumPopup("Type:", _type);
            _priority = EditorGUILayout.IntField("Priority: ", _priority);

            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Create Quest", GUILayout.Height(35))) CreateConversation();
        }

        private string GetCurrentFolder()
        {
            Type projectWindowUtilType = typeof(ProjectWindowUtil);
            MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            return getActiveFolderPath is not null 
                ? getActiveFolderPath.Invoke(null, new object[0]).ToString()
                : string.Empty;
        }
        
        private string GetCurrentName()
        {
            string[] subfolders = _folder.Split(Path.DirectorySeparatorChar);
            return subfolders.Length > 0 ? subfolders[^1] : string.Empty;
        }

        private void CreateConversation()
        {
            Type questType = TypeIndex.QuestTypes[_questTypeIndex];
            Quest quest = CreateInstance(questType) as Quest;
            if (!quest) return;

            quest.name = _name;
            quest.Name = _name;
            quest.Description = _description;
            quest.Conclusion = _conclusion;
            quest.Type = _type;
            quest.Priority = _priority;
            
            CreateFolders();
            string path = Path.Combine(_folder, _name + ".asset");
            
            AssetDatabase.CreateAsset(quest, path);
            EditorUtility.SetDirty(quest);
            AssetDatabase.SaveAssetIfDirty(quest);
            AssetDatabase.Refresh();

            Close();
            
            QuestsWindow.Open(quest).Focus();
        }

        private void CreateFolders()
        {
            string[] subfolders = _folder.Split(Path.DirectorySeparatorChar);
            if (subfolders.Length == 0) return;
            string parent = subfolders[0];
            for (int index = 1; index < subfolders.Length; index++)
            {
                var subfolder = subfolders[index];
                string child = Path.Join(parent, subfolder);
                if (!AssetDatabase.IsValidFolder(child)) AssetDatabase.CreateFolder(parent, subfolder);
                parent = child;
            }
        }
    }
}