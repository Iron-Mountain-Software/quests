using IronMountain.Conditions.Editor;
using IronMountain.ScriptableActions.Editor;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    [CustomEditor(typeof(QuestRequirement), true)]
    public class QuestRequirementInspector : StyledInspector
    {
        private QuestRequirement _questRequirement;
        private ScriptableActionsEditor _onTrackActionsEditor;
        private ConditionEditor _completionConditionEditor;
        private ConditionEditor _failConditionEditor;
        private ScriptableActionsEditor _onCompleteActionsEditor;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            if (target) _questRequirement = (QuestRequirement) target;
            _onTrackActionsEditor = new ScriptableActionsEditor("On Track", _questRequirement, _questRequirement.ActionsOnTrack);
            _completionConditionEditor = new ConditionEditor("To Complete", _questRequirement,
                newCondition => _questRequirement.Condition = newCondition);
            _failConditionEditor = new ConditionEditor("To Fail", _questRequirement,
                newCondition => _questRequirement.FailCondition = newCondition);
            _onCompleteActionsEditor = new ScriptableActionsEditor("On Complete", _questRequirement, _questRequirement.ActionsOnComplete);
        }
        
        public override void OnInspectorGUI()
        {
            DrawDescriptions();
            DrawDependencies();
            _onTrackActionsEditor.Draw();
            _completionConditionEditor.Draw(ref _questRequirement.condition);
            _failConditionEditor.Draw(ref _questRequirement.failCondition);
            _onCompleteActionsEditor.Draw();
            DrawOtherProperties();
            DrawEditorActionButtons();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDescriptions()
        {
            bool errors = _questRequirement.DescriptionHasErrors;
            EditorGUILayout.BeginHorizontal(errors ? HeaderInvalid : HeaderValid, GUILayout.ExpandWidth(true));
            GUILayout.Label("Descriptions", errors ? H1Invalid : H1Valid, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultDetail"), new GUIContent("Detail"));
            EditorGUI.indentLevel += 2;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("detail"), GUIContent.none);
            EditorGUI.indentLevel -= 2;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultTip"), new GUIContent("Tip"));
            EditorGUI.indentLevel += 2;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tip"), GUIContent.none);
            EditorGUI.indentLevel -= 2;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("depiction"));
        }

        private void DrawDependencies()
        {
            GUILayout.Space(10);
            SerializedProperty list = serializedObject.FindProperty("dependencies");

            bool errors = _questRequirement.DependenciesHaveErrors;
            EditorGUILayout.BeginHorizontal(errors ? HeaderInvalid : HeaderValid, GUILayout.ExpandWidth(true));
            GUILayout.Label("Dependencies", errors ? H1Invalid : H1Valid, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Add", GUILayout.MaxWidth(125)))
            {
                GenericMenu menu = new GenericMenu();
                var addDependency = new GenericMenu.MenuFunction2(dependency =>
                    {
                        if (dependency is QuestRequirement dependencyToAdd)
                        {
                            list.InsertArrayElementAtIndex(list.arraySize);
                            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = dependencyToAdd;
                        }
                    }
                );
                foreach (QuestRequirement possibleDependency in _questRequirement.Quest.Requirements)
                {
                    if (possibleDependency != _questRequirement && !_questRequirement.Dependencies.Contains(possibleDependency))
                        menu.AddItem(new GUIContent("Add " + possibleDependency.name), false, addDependency, possibleDependency);
                }
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < list.arraySize; i++)
            {
                QuestRequirement dependency = (QuestRequirement) list.GetArrayElementAtIndex(i).objectReferenceValue;
                if (!dependency) continue;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label((i + 1) + ". " + dependency.name);
                switch (dependency.State)
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
                }
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(70)))
                {
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
            EditorGUILayout.BeginHorizontal(HeaderValid,GUILayout.ExpandWidth(true));
            GUILayout.Label("Other", H1Valid, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            DrawPropertiesExcluding(serializedObject,
                "m_Script",
                "id",
                "quest",
                "defaultDetail",
                "detail",
                "defaultTip",
                "tip",
                "depiction",
                "dependencies",
                "actionsOnTrack",
                "actionsOnComplete",
                "condition",
                "failCondition"
            );
        }
        
        protected virtual void DrawEditorActionButtons()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(HeaderValid,GUILayout.ExpandWidth(true));
            GUILayout.Label("Editor Actions", H1Valid, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Tracking", GUILayout.MinHeight(25)) && _questRequirement)
            {
                _questRequirement.StartTracking();
            }
            if (GUILayout.Button("Delete Requirement", GUILayout.MinHeight(25)))
            {
                QuestRequirementsEditor.RemoveRequirementFromQuest(_questRequirement);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}