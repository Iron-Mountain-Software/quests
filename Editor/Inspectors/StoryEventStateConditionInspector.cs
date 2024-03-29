using IronMountain.Conditions.Editor;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor.Inspectors
{
    [CustomEditor(typeof(StoryEventStateCondition), true)]
    public class StoryEventStateConditionInspector : ConditionInspector
    {
        protected override void DrawProperties()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("storyEvent"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("comparisonType"), GUIContent.none, GUILayout.MaxWidth(38));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("state"), GUIContent.none, GUILayout.MaxWidth(90));
            GUILayout.EndHorizontal();
        }
    }
}