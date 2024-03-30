using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor.Inspectors
{
    [CustomEditor(typeof(StoryEventController))]
    public class StoryEventControllerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space(5);
            serializedObject.Update();
            SerializedProperty list = serializedObject.FindProperty("controllerEvents");

            for (int i = 0; i < list.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i).FindPropertyRelative("storyEvent"), GUIContent.none);
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i).FindPropertyRelative("action"), GUIContent.none, GUILayout.Width(110));
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i).FindPropertyRelative("trigger"), GUIContent.none, GUILayout.Width(90));

                bool hasUpArrow = i > 0;
                bool hasDownArrow = list.arraySize > 1 && i < list.arraySize - 1 ;
                
                if (hasUpArrow && GUILayout.Button("↑", GUILayout.Width(hasDownArrow ? 18 : 40)))
                {
                    list.MoveArrayElement(i, i - 1);
                }
                if (hasDownArrow && GUILayout.Button("↓", GUILayout.Width(hasUpArrow ? 18 : 40)))
                {
                    list.MoveArrayElement(i, i + 1);
                }
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), GUILayout.MaxWidth(25)))
                {
                    list.DeleteArrayElementAtIndex(i);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Add New Controller Event", GUILayout.MaxWidth(200) ))
            {
                list.InsertArrayElementAtIndex(list.arraySize);
            }
            EditorGUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
