using IronMountain.Conditions.Editor;
using IronMountain.ScriptableActions.Editor;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    [CustomEditor(typeof(Quest), true)]
    public class QuestInspector : UnityEditor.Editor
    {
        [Header("Cache")]
        private Quest _quest;
        private ConditionEditor _prerequisitesEditor;
        private ScriptableActionsEditor _onStartActionsEditor;
        private ScriptableActionsEditor _onCompleteActionsEditor;
        private QuestRequirement _selectedQuestRequirement;
        private UnityEditor.Editor _cachedSelectedRequirementEditor;

        private void OnEnable()
        {
            _quest = (Quest) target;
            _prerequisitesEditor = new ConditionEditor("Prerequisites", _quest,
                newCondition => _quest.Prerequisites = newCondition);
            _onStartActionsEditor = new ScriptableActionsEditor("On Start", _quest, _quest.ActionsOnActivate);
            _onCompleteActionsEditor = new ScriptableActionsEditor("On Complete", _quest, _quest.ActionsOnComplete);
        }

        private void StartBox(string title)
        {
            GUILayout.Space(10);
            //EditorGUILayout.BeginVertical(Core.Editor.Styles.Subbox, GUILayout.MinHeight(75));
            //GUILayout.Label(title, Core.Editor.Styles.H3, GUILayout.ExpandWidth(true));
            EditorGUI.indentLevel++;
        }

        private void EndBox()
        {
            EditorGUI.indentLevel--;
            //EditorGUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            _selectedQuestRequirement = QuestInspectorHeader.Draw(_quest, _selectedQuestRequirement);
            GUILayout.Space(10);

            if (_selectedQuestRequirement)
            {
                CreateCachedEditor(_selectedQuestRequirement, null, ref _cachedSelectedRequirementEditor);
                _cachedSelectedRequirementEditor.OnInspectorGUI();
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("localizedName"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("conclusion"));
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"));

                _prerequisitesEditor.Draw(ref _quest.prerequisites);

                _onStartActionsEditor.Draw();

                StartBox("Requirements");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("requirements"));
                EndBox();
                
                _onCompleteActionsEditor.Draw();
                
                StartBox("Rewards");
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("rewardCoins"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("rewardXP"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("rewardItems"));
                EndBox();

                DrawInheritedProperties();
                
                if (GUILayout.Button("Copy Quest Content", GUILayout.MinHeight(25)))
                {
                    //EditorGUIUtility.systemCopyBuffer = QuestPrinter.PrintQuest(_quest);
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        protected virtual void DrawInheritedProperties() { }
    }
}
