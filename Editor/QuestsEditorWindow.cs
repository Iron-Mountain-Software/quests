using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IronMountain.Quests.Editor
{
    public class QuestsEditorWindow : EditorWindow
    {
        private static QuestsEditorWindow Current { get; set; }
        
        private enum QuestFilter
        {
            All,
            Main,
            Side
        }

        private QuestFilter _questFilter = QuestFilter.All;
        
        private Vector2 _sidebarScroll = Vector2.zero;
        private Vector2 _contentScroll = Vector2.zero;

        private int _sidebarWidth = 220;

        private Rect _sidebarSection;
        private Rect _contentSection;

        [Header("Cache")]
        private readonly List<Quest> _quests = new();
        private Quest _selectedQuest;
        private QuestRequirement _selectedQuestRequirement;
        private UnityEditor.Editor _cachedEditor;
        
        [OnOpenAsset(-1)]
        public static bool Open(int instanceID, int line)
        {
            Object asset = EditorUtility.InstanceIDToObject(instanceID);
            if (asset is Quest quest)
            {
                Open(quest);
                return true;
            }
            if (asset is QuestRequirement questRequirement)
            {
                Open(questRequirement.Quest, questRequirement);
                return true;
            }
            return false;
        }
        
        public static void Open(Quest selectedQuest = null, QuestRequirement selectedQuestRequirement = null)
        {
            Current = GetWindow<QuestsEditorWindow>("Quests", true);
            Current.minSize = new Vector2(700, 700);
            Current._selectedQuest = selectedQuest;
            Current._selectedQuestRequirement = selectedQuestRequirement;
            Current.RefreshQuestsList();
        }

        private void RefreshQuestsList()
        {
            _quests.Clear();
            AssetDatabase.Refresh();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(Quest)}");
            for( int i = 0; i < guids.Length; i++ )
            {
                string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
                Quest quest = AssetDatabase.LoadAssetAtPath<Quest>( assetPath );
                if (quest) _quests.Add(quest);
            }
        }

        private void OnGUI()
        {
            Current = this;
            DrawLayouts();
            
            GUILayout.BeginArea(_sidebarSection);
            DrawSidebar();
            GUILayout.EndArea();
            
            GUILayout.BeginArea(_contentSection);
            if (_selectedQuest)
            {
                _selectedQuestRequirement = QuestInspectorHeader.Draw(_selectedQuest, _selectedQuestRequirement);
                _contentScroll = GUILayout.BeginScrollView(_contentScroll);
                if (_selectedQuestRequirement)
                {
                    UnityEditor.Editor.CreateCachedEditor(_selectedQuestRequirement, null, ref _cachedEditor);
                }
                else UnityEditor.Editor.CreateCachedEditor(_selectedQuest, null, ref _cachedEditor);
                _cachedEditor.OnInspectorGUI();
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No quest selected.");
                _cachedEditor = null;
            }
            GUILayout.EndArea();
        }

        private void DrawLayouts()
        {
            _sidebarSection.x = 0;
            _sidebarSection.y = 0;
            _sidebarSection.width = _sidebarWidth;
            _sidebarSection.height = Current.position.height;
            
            _contentSection.x = _sidebarWidth;
            _contentSection.y = 0;
            _contentSection.width = Current.position.width - _sidebarWidth;
            _contentSection.height = Current.position.height;
        }

        private void SelectQuest(Quest quest)
        {
            Selection.activeObject = quest;
            GUI.FocusControl(null);
            _selectedQuest = quest;
            _selectedQuestRequirement = null;
        }
        
        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            
            if (GUILayout.Button("Refresh"))
            {
                RefreshQuestsList();
            }
            
            if (GUILayout.Button("Create New"))
            {
                AddQuestMenu.Open();
                RefreshQuestsList();
            }
            
            EditorGUILayout.Space();
            DrawFilterButtons();
            EditorGUILayout.Space();
            
            _quests.Sort(Comparison);

            _sidebarScroll.x = 0;
            _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll, false, true);
            int _lastPriority = int.MinValue;
            foreach(Quest quest in _quests)
            {
                if (!quest
                    || _questFilter == QuestFilter.Main && quest.Type != Quest.StoryType.Main
                    || _questFilter == QuestFilter.Side && quest.Type != Quest.StoryType.Side) continue;
                if (quest.Priority / 100 > _lastPriority / 100)
                {
                    EditorGUILayout.Space(10);
                    _lastPriority = quest.Priority;
                }

                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(_sidebarWidth - 20));
                string questName = quest.name.Trim(' ', '_');
                EditorGUI.BeginDisabledGroup(quest == _selectedQuest);
                if (GUILayout.Button(" " + quest.Priority + ". " + questName, GUILayout.MaxHeight(25)))
                    SelectQuest(quest);
                GUILayout.Label(quest.HasErrors()
                        ? new GUIContent(EditorGUIUtility.IconContent("console.erroricon"))
                        : new GUIContent(EditorGUIUtility.IconContent("TestPassed")), 
                    GUILayout.MaxWidth(15), GUILayout.MaxHeight(25));
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawFilterButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginDisabledGroup(_questFilter == QuestFilter.All);
            if (GUILayout.Button("All")) _questFilter = QuestFilter.All;
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(_questFilter == QuestFilter.Main);
            if (GUILayout.Button("Main")) _questFilter = QuestFilter.Main;
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(_questFilter == QuestFilter.Side);
            if (GUILayout.Button("Side")) _questFilter = QuestFilter.Side;
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndHorizontal();
        }

        private int Comparison(Quest a, Quest b)
        {
            if (!a) return -1;
            if (!b) return 1;
            return a.Priority != b.Priority
                ? a.Priority.CompareTo(b.Priority)
                : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        }
    }
}
