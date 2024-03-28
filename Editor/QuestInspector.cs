using IronMountain.Conditions.Editor;
using IronMountain.ScriptableActions.Editor;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    [CustomEditor(typeof(Quest), true)]
    public class QuestInspector : StyledInspector
    {
        public QuestRequirement selectedQuestRequirement;

        private Quest _quest;
        private ConditionEditor _prerequisitesEditor;
        private ScriptableActionsEditor _onActivateActionsEditor;
        private ScriptableActionsEditor _onCompleteActionsEditor;
        
        private Vector2 _selectedQuestRequirementScroll;
        private UnityEditor.Editor _cachedEditor;

        private bool _localize;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (target) _quest = (Quest) target;
            _prerequisitesEditor = new ConditionEditor("Prerequisites", _quest,
                newCondition => _quest.Prerequisites = newCondition);
            _onActivateActionsEditor = new ScriptableActionsEditor("On Activation", _quest, _quest.ActionsOnActivate);
            _onCompleteActionsEditor = new ScriptableActionsEditor("On Completion", _quest, _quest.ActionsOnComplete);
        }

        public override void OnInspectorGUI()
        {
            DrawMainData();
            _prerequisitesEditor.Draw(ref _quest.prerequisites);
            _onActivateActionsEditor.Draw();
            _onCompleteActionsEditor.Draw();
            DrawRequirements();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMainData()
        {
            bool errors = _quest.DescriptionHasErrors;
            EditorGUILayout.BeginHorizontal(errors ? HeaderInvalid : HeaderValid, GUILayout.ExpandWidth(true));
            GUILayout.Label(_quest.name, errors ? H1Invalid : H1Valid, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
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
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultName"), new GUIContent("Name"));
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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"));
            DrawPropertiesExcluding(serializedObject,
                "m_Script",
                "id",
                "defaultName",
                "localizedName",
                "defaultDescription",
                "description",
                "defaultConclusion",
                "conclusion",
                "type",
                "priority",
                "prerequisites",
                "actionsOnActivate",
                "actionsOnComplete",
                "requirements"
            );
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(10));
            EditorGUILayout.Space(7);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            switch (_quest.State)
            {
                case Quest.StateType.None:
                    GUILayout.Label("Not Tracking", NotTracking, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(40));
                    break;
                case Quest.StateType.Active:
                    GUILayout.Label("Active", Tracking, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(40));
                    break;
                case Quest.StateType.Completed:
                    GUILayout.Label("Completed", Completed, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(40));
                    break;
            }
            if (GUILayout.Button("Activate", GUILayout.ExpandHeight(true), GUILayout.MaxHeight(40)) && _quest)
            {
                _quest.Activate();
            }
            if (GUILayout.Button("Complete", GUILayout.ExpandHeight(true), GUILayout.MaxHeight(40)) && _quest)
            {
                _quest.Complete();
            }
            if (GUILayout.Button("Log & Copy Data", GUILayout.ExpandHeight(true), GUILayout.MaxHeight(40)))
            {
                string documentation = _quest.WriteDocumentation();
                Debug.Log(documentation);
                EditorGUIUtility.systemCopyBuffer = documentation;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
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
                EditorGUILayout.BeginHorizontal(GUILayout.Height(30));
                
                GUILayout.Label(requirement.HasErrors()
                        ? new GUIContent(EditorGUIUtility.IconContent("console.erroricon"))
                        : new GUIContent(EditorGUIUtility.IconContent("TestPassed")), 
                    GUILayout.Width(20), GUILayout.Height(20));

                if (GUILayout.Button(requirement.Detail, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    selectedQuestRequirement = requirement;
                }

                switch (requirement.State)
                {
                    case QuestRequirement.StateType.None:
                        GUILayout.Label("Not\nTracking", NotTracking, GUILayout.Width(70), GUILayout.ExpandHeight(true));
                        break;
                    case QuestRequirement.StateType.Tracking:
                        GUILayout.Label("Tracking", Tracking, GUILayout.Width(70), GUILayout.ExpandHeight(true));
                        break;
                    case QuestRequirement.StateType.Completed:
                        GUILayout.Label("Completed", Completed, GUILayout.Width(70), GUILayout.ExpandHeight(true));
                        break;
                    case QuestRequirement.StateType.Failed:
                        GUILayout.Label("Failed", Failed, GUILayout.Width(70), GUILayout.ExpandHeight(true));
                        break;
                }
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(25));
                if (GUILayout.Button("↑"))
                {
                    list.MoveArrayElement(i, i - 1);
                }
                if (GUILayout.Button("↓"))
                {
                    list.MoveArrayElement(i, i + 1);
                }
                EditorGUILayout.EndVertical();
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), GUILayout.MaxWidth(25), GUILayout.ExpandHeight(true)))
                {
                    if (selectedQuestRequirement == requirement) selectedQuestRequirement = null;
                    QuestRequirementsEditor.RemoveRequirementFromQuest(requirement);
                    list.DeleteArrayElementAtIndex(i);
                    AssetDatabase.SaveAssets();
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
