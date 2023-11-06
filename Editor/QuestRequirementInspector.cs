using IronMountain.Conditions.Editor;
using IronMountain.ScriptableActions.Editor;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    [CustomEditor(typeof(QuestRequirement), true)]
    public class QuestRequirementInspector : UnityEditor.Editor
    {
        private ScriptableActionsEditor _onTrackActionsEditor;
        private ConditionEditor _completionConditionEditor;
        private ScriptableActionsEditor _onCompleteActionsEditor;

        private QuestRequirement _questRequirement;

        private void OnEnable()
        {
            _questRequirement = (QuestRequirement) target;
            _onTrackActionsEditor = new ScriptableActionsEditor("On Track", _questRequirement, _questRequirement.ActionsOnTrack);
            _completionConditionEditor = new ConditionEditor("To Complete", _questRequirement,
                newCondition => _questRequirement.Condition = newCondition);
            _onCompleteActionsEditor = new ScriptableActionsEditor("On Complete", _questRequirement, _questRequirement.ActionsOnComplete);
        }
        
        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("detail"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tip"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("location"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("depiction"));
            DrawDependencies();
            GUILayout.Space(20);
            _onTrackActionsEditor.Draw();
            _completionConditionEditor.Draw(ref _questRequirement.condition);
            _onCompleteActionsEditor.Draw();
            DrawInheritedProperties();
            serializedObject.ApplyModifiedProperties();
            GUILayout.Space(5);
            if (GUILayout.Button("Delete Requirement", GUILayout.MinHeight(25)))
            {
                QuestRequirementsEditor.RemoveRequirementFromQuest(_questRequirement);
            }
        }

        private void DrawDependencies()
        {
            GUILayout.Space(8);
            EditorGUILayout.BeginVertical(GUILayout.MinHeight(70));
    
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Dependencies");
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(25));
            if (GUILayout.Button("＋")) 
            {
                GenericMenu menu = new GenericMenu();
                var addDependency = new GenericMenu.MenuFunction2(dependency =>
                    {
                        if (dependency is QuestRequirement dependencyToAdd)
                            _questRequirement.Dependencies.Add(dependencyToAdd);
                    }
                );
                foreach (QuestRequirement possibleDependency in _questRequirement.Quest.Requirements)
                {
                    if (possibleDependency != _questRequirement && !_questRequirement.Dependencies.Contains(possibleDependency))
                        menu.AddItem(new GUIContent("Add " + possibleDependency.name), false, addDependency, possibleDependency);
                }
                menu.ShowAsContext();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
    
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dependencies"), GUIContent.none);
            EditorGUI.indentLevel--;
    
            EditorGUILayout.EndVertical();
        }

        protected virtual void DrawInheritedProperties() { }
    }
}