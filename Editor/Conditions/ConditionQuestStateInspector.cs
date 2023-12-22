using IronMountain.Conditions.Editor;
using IronMountain.Quests.Conditions;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor.Conditions
{
    [CustomEditor(typeof(ConditionQuestState), true)]
    public class ConditionQuestStateInspector : ConditionInspector
    {
        protected override void DrawProperties()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("quest"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("comparisonType"), GUIContent.none, GUILayout.MaxWidth(38));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("state"), GUIContent.none, GUILayout.MaxWidth(90));
            GUILayout.EndHorizontal();
        }
    }
}