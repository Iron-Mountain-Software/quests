using IronMountain.Conditions.Editor;
using IronMountain.Quests.Conditions;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor.Conditions
{
    [CustomEditor(typeof(ConditionQuestRequirementState), true)]
    public class ConditionQuestRequirementStateInspector : ConditionInspector
    {
        protected override void DrawProperties()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("requirement"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("comparisonType"), GUIContent.none, GUILayout.MaxWidth(38));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("state"), GUIContent.none, GUILayout.MaxWidth(90));
            GUILayout.EndHorizontal();
        }
    }
}