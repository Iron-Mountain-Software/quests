using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronMountain.Quests.Editor.Inspectors;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IronMountain.Quests.Editor.Windows
{
    public class QuestsWindow : EditorWindow
    {
        private static QuestsWindow Current { get; set; }
        
        private enum SortType
        {
            Priority,
            Path
        }

        private static bool _filterEnabled = true;
        private static Quest.StoryType _questFilter = Quest.StoryType.Main;
        private static SortType _sortType = SortType.Path;
        
        private Vector2 _sidebarScroll = Vector2.zero;
        private Vector2 _contentScroll = Vector2.zero;

        private int _sidebarWidth = 220;

        private Rect _sidebarSection;
        private Rect _contentSection;

        [Header("Cache")]
        private readonly List<Quest> _quests = new();
        private Quest _selectedQuest;
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
        
        public static QuestsWindow Open(Quest selectedQuest = null, QuestRequirement selectedQuestRequirement = null)
        {
            Current = GetWindow<QuestsWindow>("Quests", true);
            Current.minSize = new Vector2(700, 700);
            Current.Select(selectedQuest, selectedQuestRequirement);
            Current.RefreshIndex();
            return Current;
        }
        
        private void OnEnable()
        {
            QuestsManager.OnQuestsChanged += OnQuestsChanged;
        }

        private void OnFocus() => RefreshIndex();

        private void OnDisable()
        {
            QuestsManager.OnQuestsChanged -= OnQuestsChanged;
        }

        private void OnQuestsChanged()
        {
            foreach (Quest quest in QuestsManager.Quests)
            {
                if (!quest || _quests.Contains(quest)) continue;
                _quests.Add(quest);
            }
            Focus();
        }

        private void RefreshIndex()
        {
            _quests.Clear();
            AssetDatabase.Refresh();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(Quest)}");
            for( int i = 0; i < guids.Length; i++ )
            {
                string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
                Quest quest = AssetDatabase.LoadAssetAtPath<Quest>( assetPath );
                if (!quest || _quests.Contains(quest)) continue;
                _quests.Add(quest);
            }
        }

        private void OnGUI()
        {
            Current = this;
            DrawLayouts();
            
            DrawSidebar();
            
            GUILayout.BeginArea(_contentSection);
            if (_selectedQuest)
            {
                _contentScroll = GUILayout.BeginScrollView(_contentScroll);
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

        private void Select(Quest quest, QuestRequirement questRequirement)
        {
            Selection.activeObject = quest;
            GUI.FocusControl(null);
            _selectedQuest = quest;
            UnityEditor.Editor.CreateCachedEditor(_selectedQuest, null, ref _cachedEditor);
            if (_cachedEditor is QuestInspector questInspector)
            {
                questInspector.selectedQuestRequirement = questRequirement;
            }
        }
        
        private void DrawSidebar()
        {
            GUILayout.BeginArea(_sidebarSection);
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            
            if (GUILayout.Button("Create", GUILayout.Height(40))) NewQuestWindow.Open();
            
            EditorGUILayout.Space(4);
            
            DrawSortButtons();
            DrawFilterButtons();
            
            EditorGUILayout.Space();

            switch (_sortType)
            {
                case SortType.Path:
                    _quests.Sort(ComparePath);
                    break;
                case SortType.Priority:
                    _quests.Sort(ComparePriority);
                    break;
                default:
                    _quests.Sort(ComparePath);
                    break;
            }

            _sidebarScroll.x = 0;
            _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll, false, true);
            int _lastPriority = int.MinValue;
            string lastDirectory = string.Empty;
            foreach(Quest quest in _quests)
            {
                if (!quest) continue;
                
                if (_filterEnabled && quest.Type != _questFilter) continue;

                if (_sortType == SortType.Path)
                {
                    string path = AssetDatabase.GetAssetPath(quest).Split(Path.DirectorySeparatorChar).ToList()[^3];
                    if (path != lastDirectory)
                    {
                        EditorGUILayout.LabelField(path);
                        lastDirectory = path;
                    }
                }

                if (_sortType == SortType.Priority)
                {
                    if (quest.Priority / 100 > _lastPriority / 100)
                    {
                        EditorGUILayout.Space(10);
                        _lastPriority = quest.Priority;
                    }
                }

                DrawSideBarQuestItem(quest);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawSideBarQuestItem(Quest quest)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(_sidebarWidth - 20));
            string questName = quest.name.Trim(' ', '_');
            EditorGUI.BeginDisabledGroup(quest == _selectedQuest);
            string label = _sortType == SortType.Priority
                ? " " + quest.Priority + ". " + questName
                : questName;
            if (GUILayout.Button(label, GUILayout.MaxHeight(25)))
                Select(quest, null);
            GUILayout.Label(quest.HasErrors()
                    ? new GUIContent(EditorGUIUtility.IconContent("console.erroricon"))
                    : new GUIContent(EditorGUIUtility.IconContent("TestPassed")), 
                GUILayout.MaxWidth(15), GUILayout.MaxHeight(25));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        private void DrawFilterButtons()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter:", GUILayout.Width(55));
            _filterEnabled = EditorGUILayout.Toggle(GUIContent.none, _filterEnabled, GUILayout.Width(15));
            EditorGUI.BeginDisabledGroup(!_filterEnabled);
            EditorGUI.BeginDisabledGroup(_questFilter == Quest.StoryType.Main);
            if (GUILayout.Button("Main")) _questFilter = Quest.StoryType.Main;
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(_questFilter == Quest.StoryType.Side);
            if (GUILayout.Button("Side")) _questFilter = Quest.StoryType.Side;
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawSortButtons()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sort:", GUILayout.Width(55));
            EditorGUI.BeginDisabledGroup(_sortType == SortType.Path);
            if (GUILayout.Button("Path")) _sortType = SortType.Path;
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(_sortType == SortType.Priority);
            if (GUILayout.Button("Priority")) _sortType = SortType.Priority;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        
        private int ComparePath(Quest a, Quest b)
        {
            if (!a) return -1;
            if (!b) return 1;
            return string.Compare(AssetDatabase.GetAssetPath(a), AssetDatabase.GetAssetPath(b), StringComparison.Ordinal);
        }

        private int ComparePriority(Quest a, Quest b)
        {
            if (!a) return -1;
            if (!b) return 1;
            return a.Priority != b.Priority
                ? a.Priority.CompareTo(b.Priority)
                : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        }
    }
}
