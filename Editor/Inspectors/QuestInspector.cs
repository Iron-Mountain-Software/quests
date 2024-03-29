using IronMountain.Quests.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor.Inspectors
{
    [CustomEditor(typeof(Quest), true)]
    public class QuestInspector : StoryEventInspector
    {
        public QuestRequirement selectedQuestRequirement;

        private Quest _quest;

        private Vector2 _selectedQuestRequirementScroll;
        private UnityEditor.Editor _cachedEditor;
        private GUIContent _errorIconContent = new (EditorGUIUtility.IconContent("console.erroricon"));

        private bool _localize;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (target) _quest = (Quest) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawRequirements();
        }

        protected override void DrawMainSection()
        {
            EditorGUILayout.BeginHorizontal(HeaderValid, GUILayout.ExpandWidth(true));
            GUILayout.Label(_quest.name, H1Valid, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            DrawMainData();
            EditorGUILayout.BeginVertical(GUILayout.Width(10));
            EditorGUILayout.Space(7);
            EditorGUILayout.EndVertical();
            DrawStateControls(false);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawMainData()
        {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!_localize);
            if (GUILayout.Button("Simple")) _localize = false;
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(_localize);
            if (GUILayout.Button("Localized")) _localize = true;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (!_localize)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultName"), new GUIContent("Name"));
                EditorGUI.BeginDisabledGroup(_quest.name == _quest.Name);
                if (GUILayout.Button("Rename Asset", GUILayout.Width(110)))
                {
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(_quest), _quest.Name);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultDescription"), new GUIContent("Description"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultConclusion"), new GUIContent("Conclusion"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("localizedName"), new GUIContent("Name"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent("Description"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("conclusion"), new GUIContent("Conclusion"));
            }
            
            GUILayout.Space(5);
            
            DrawPropertiesExcluding(serializedObject,
                "m_Script",
                "id",
                "defaultName",
                "localizedName",
                "defaultDescription",
                "description",
                "defaultConclusion",
                "conclusion",
                "actionsOnActivate",
                "actionsOnComplete",
                "actionsOnFail",
                "prerequisites",
                "completionCondition",
                "failCondition",
                "requirements"
            );

            EditorGUILayout.EndVertical();
        }

        private void DrawRequirements()
        {
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal(HeaderValid, GUILayout.ExpandWidth(true));
            GUILayout.Label("Requirements", H1Valid, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            DrawRequirementsSidebar();
            EditorGUILayout.BeginVertical(GUILayout.Width(10));
            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();
            DrawSelectedQuestRequirement();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRequirementsSidebar()
        {
            serializedObject.Update();
            SerializedProperty list = serializedObject.FindProperty("requirements");

            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(200), GUILayout.MinHeight(20));
            for (int i = 0; i < list.arraySize; i++)
            {
                QuestRequirement requirement = (QuestRequirement) list.GetArrayElementAtIndex(i).objectReferenceValue;
                if (!requirement) continue;
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(30));

                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(25), GUILayout.Height(30));
                if (i > 0 && GUILayout.Button("↑", GUILayout.ExpandHeight(true)))
                {
                    list.MoveArrayElement(i, i - 1);
                }
                if (list.arraySize > 1 && i < list.arraySize - 1 && GUILayout.Button("↓", GUILayout.ExpandHeight(true)))
                {
                    list.MoveArrayElement(i, i + 1);
                }
                EditorGUILayout.EndVertical();
                
                EditorGUI.BeginDisabledGroup(selectedQuestRequirement == requirement);
                if (GUILayout.Button(requirement.Detail, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    selectedQuestRequirement = requirement;
                    GUI.FocusControl(null);
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), GUILayout.MaxWidth(25), GUILayout.ExpandHeight(true)))
                {
                    if (selectedQuestRequirement == requirement) selectedQuestRequirement = null;
                    list.DeleteArrayElementAtIndex(i);
                    QuestRequirementsEditor.RemoveRequirementFromQuest(requirement);
                    AssetDatabase.SaveAssets();
                    return;
                }
                
                switch (requirement.State)
                {
                    case StoryEvent.StateType.Inactive:
                        GUILayout.Label("Inactive", Inactive, GUILayout.Width(60), GUILayout.ExpandHeight(true));
                        break;
                    case StoryEvent.StateType.Active:
                        GUILayout.Label("Active", Active, GUILayout.Width(60), GUILayout.ExpandHeight(true));
                        break;
                    case StoryEvent.StateType.Complete:
                        GUILayout.Label("Complete", Complete, GUILayout.Width(60), GUILayout.ExpandHeight(true));
                        break;
                    case StoryEvent.StateType.Failed:
                        GUILayout.Label("Fail", Fail, GUILayout.Width(60), GUILayout.ExpandHeight(true));
                        break;
                }

                if (requirement.HasErrors())
                {
                    GUILayout.Label(_errorIconContent, GUILayout.Width(20), GUILayout.ExpandHeight(true));
                }

                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add", GUILayout.Height(30)))
                AddQuestRequirementMenu.Open(_quest);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSelectedQuestRequirement()
        {
            EditorGUILayout.BeginVertical();
            if (selectedQuestRequirement)
            {
                _selectedQuestRequirementScroll = GUILayout.BeginScrollView(_selectedQuestRequirementScroll);
                CreateCachedEditor(selectedQuestRequirement, null, ref _cachedEditor);
                if (_cachedEditor is QuestRequirementInspector questRequirementInspector)
                {
                    questRequirementInspector.localize = _localize;
                }
                _cachedEditor.OnInspectorGUI();
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No quest requirement selected.");
                _cachedEditor = null;
            }
            EditorGUILayout.EndVertical();
        }
    }
}
