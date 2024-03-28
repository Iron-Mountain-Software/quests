using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    [CustomEditor(typeof(QuestRequirement), true)]
    public class QuestRequirementInspector : StoryEventEditor
    {
        private QuestRequirement _questRequirement;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            if (target) _questRequirement = (QuestRequirement) target;
        }

        protected override void DrawMainSection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            if (!localize)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultDetail"), new GUIContent("Detail"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultTip"), new GUIContent("Tip"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("detail"), new GUIContent("Detail"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tip"), new GUIContent("Tip"));
            }

            DrawPropertiesExcluding(serializedObject,
                "m_Script",
                "id",
                "quest",
                "defaultDetail",
                "detail",
                "defaultTip",
                "tip",
                "actionsOnActivate",
                "actionsOnComplete",
                "actionsOnFail",
                "prerequisites",
                "completionCondition",
                "failCondition"
            );
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(10));
            EditorGUILayout.Space(7);
            EditorGUILayout.EndVertical();

            DrawStateControls(_questRequirement.Quest.State is not StoryEvent.StateType.Active);
            EditorGUILayout.EndHorizontal();
        }
    }
}