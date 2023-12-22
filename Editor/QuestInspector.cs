using IronMountain.Conditions.Editor;
using IronMountain.ScriptableActions.Editor;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    [CustomEditor(typeof(Quest), true)]
    public class QuestInspector : StyledInspector
    {
        private Quest _quest;
        private ConditionEditor _prerequisitesEditor;
        private ScriptableActionsEditor _onActivateActionsEditor;
        private ScriptableActionsEditor _onCompleteActionsEditor;

        protected override void OnEnable()
        {
            base.OnEnable();
            _quest = (Quest) target;
            _prerequisitesEditor = new ConditionEditor("Prerequisites", _quest,
                newCondition => _quest.Prerequisites = newCondition);
            _onActivateActionsEditor = new ScriptableActionsEditor("On Activation", _quest, _quest.ActionsOnActivate);
            _onCompleteActionsEditor = new ScriptableActionsEditor("On Completion", _quest, _quest.ActionsOnComplete);
        }

        public override void OnInspectorGUI()
        {
            DrawDescriptions();
            
            _prerequisitesEditor.Draw(ref _quest.prerequisites);
            DrawRequirements();
            
            _onActivateActionsEditor.Draw();
            _onCompleteActionsEditor.Draw();
            
            DrawOtherProperties();
            DrawEditorActionButtons();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDescriptions()
        {
            bool errors = _quest.DescriptionHasErrors;
            EditorGUILayout.BeginHorizontal(errors ? HeaderInvalid : HeaderValid, GUILayout.ExpandWidth(true));
            GUILayout.Label("Descriptions", errors ? H1Invalid : H1Valid, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultName"), new GUIContent("Name"));
            EditorGUI.indentLevel += 2;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("localizedName"), GUIContent.none);
            EditorGUI.indentLevel -= 2;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultDescription"), new GUIContent("Description"));
            EditorGUI.indentLevel += 2;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), GUIContent.none);
            EditorGUI.indentLevel -= 2;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultConclusion"), new GUIContent("Conclusion"));
            EditorGUI.indentLevel += 2;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conclusion"), GUIContent.none);
            EditorGUI.indentLevel -= 2;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"));
        }

        private void DrawRequirements()
        {
            GUILayout.Space(10);
            SerializedProperty list = serializedObject.FindProperty("requirements");

            bool errors = _quest.RequirementsHaveErrors;
            EditorGUILayout.BeginHorizontal(errors ? HeaderInvalid : HeaderValid, GUILayout.ExpandWidth(true));
            GUILayout.Label("Requirements", errors ? H1Invalid : H1Valid, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Add", GUILayout.MaxWidth(125)))
                AddQuestRequirementMenu.Open(_quest);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginVertical(GUILayout.Height(20));
            for (int i = 0; i < list.arraySize; i++)
            {
                QuestRequirement requirement = (QuestRequirement) list.GetArrayElementAtIndex(i).objectReferenceValue;
                if (!requirement) continue;
                EditorGUILayout.BeginHorizontal();
                
                GUILayout.Label(requirement.HasErrors()
                        ? new GUIContent(EditorGUIUtility.IconContent("console.erroricon"))
                        : new GUIContent(EditorGUIUtility.IconContent("TestPassed")), 
                    GUILayout.Width(20), GUILayout.Height(20));

                GUILayout.Label((i + 1) + ". " + requirement.Detail);
                switch (requirement.State)
                {
                    case QuestRequirement.StateType.None:
                        GUILayout.Label("Not Tracking", NotTracking, GUILayout.MaxWidth(90));
                        break;
                    case QuestRequirement.StateType.Tracking:
                        GUILayout.Label("Tracking", Tracking, GUILayout.MaxWidth(90));
                        break;
                    case QuestRequirement.StateType.Completed:
                        GUILayout.Label("Completed", Completed, GUILayout.MaxWidth(90));
                        break;
                    case QuestRequirement.StateType.Failed:
                        GUILayout.Label("Failed", Completed, GUILayout.MaxWidth(90));
                        break;
                }
                if (GUILayout.Button("↑", GUILayout.MaxWidth(20)))
                {
                    list.MoveArrayElement(i, i - 1);
                }
                if (GUILayout.Button("↓", GUILayout.MaxWidth(20)))
                {
                    list.MoveArrayElement(i, i + 1);
                }
                if (GUILayout.Button("Delete", GUILayout.MaxWidth(70)))
                {
                    QuestRequirementsEditor.RemoveRequirementFromQuest(requirement);
                    list.DeleteArrayElementAtIndex(i);
                    AssetDatabase.SaveAssets();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        protected virtual void DrawOtherProperties()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(HeaderValid, GUILayout.ExpandWidth(true));
            GUILayout.Label("Other", H1Valid, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
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
        }
        
        protected virtual void DrawEditorActionButtons()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(HeaderValid, GUILayout.ExpandWidth(true));
            GUILayout.Label("Editor Actions", H1Valid, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Log & Copy Data", GUILayout.MinHeight(25)))
            {
                string documentation = _quest.WriteDocumentation();
                Debug.Log(documentation);
                EditorGUIUtility.systemCopyBuffer = documentation;
            }
            if (GUILayout.Button("Activate", GUILayout.MinHeight(25)) && _quest)
            {
                _quest.Activate();
            }
            if (GUILayout.Button("Complete", GUILayout.MinHeight(25)) && _quest)
            {
                _quest.Complete();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
