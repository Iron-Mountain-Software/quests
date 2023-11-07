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

        private GUIStyle _header;
        private GUIStyle _h1;
        private GUIStyle _notTracking;
        private GUIStyle _tracking;
        private GUIStyle _completed;
        
        private void OnEnable()
        {
            _questRequirement = (QuestRequirement) target;
            _onTrackActionsEditor = new ScriptableActionsEditor("On Track", _questRequirement, _questRequirement.ActionsOnTrack);
            _completionConditionEditor = new ConditionEditor("To Complete", _questRequirement,
                newCondition => _questRequirement.Condition = newCondition);
            _onCompleteActionsEditor = new ScriptableActionsEditor("On Complete", _questRequirement, _questRequirement.ActionsOnComplete);
            
            Texture2D headerTexture = new Texture2D(1, 1);
            headerTexture.SetPixel(0,0, new Color(0.12f, 0.12f, 0.12f));
            headerTexture.Apply();
            _header = new GUIStyle
            {
                padding = new RectOffset(5, 5, 5, 5),
                normal = new GUIStyleState
                {
                    background = headerTexture
                }
            };
            _h1 = new GUIStyle
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = new GUIStyleState
                {
                    textColor = new Color(0.36f, 0.36f, 0.36f)
                }
            };
            
            Texture2D notTrackingTexture = new Texture2D(1, 1);
            notTrackingTexture.SetPixel(0,0, new Color(0.58f, 0.58f, 0.58f));
            notTrackingTexture.Apply();
            _notTracking = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(1, 1, 1, 1),
                normal = new GUIStyleState
                {
                    background = notTrackingTexture,
                    textColor = Color.white
                }
            };
            
            Texture2D trackingTexture = new Texture2D(1, 1);
            trackingTexture.SetPixel(0,0, new Color(0.99f, 0.95f, 0.15f));
            trackingTexture.Apply();
            _tracking = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(1, 1, 1, 1),
                normal = new GUIStyleState
                {
                    background = trackingTexture,
                    textColor = new Color(0.18f, 0.18f, 0.18f)
                }
            };
            
            Texture2D completedTexture = new Texture2D(1, 1);
            completedTexture.SetPixel(0,0, new Color(0f, 0.76f, 0.08f));
            completedTexture.Apply();
            _completed = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(1, 1, 1, 1),
                normal = new GUIStyleState
                {
                    background = completedTexture,
                    textColor = Color.white
                }
            };
        }
        
        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("detail"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tip"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("depiction"));
            DrawDependencies();
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
            GUILayout.Space(10);
            SerializedProperty list = serializedObject.FindProperty("dependencies");

            EditorGUILayout.BeginHorizontal(_header,GUILayout.ExpandWidth(true));
            GUILayout.Label("Dependencies", _h1, GUILayout.ExpandWidth(true));
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
                        GUILayout.Label("Not Tracking", _notTracking, GUILayout.MaxWidth(90));
                        break;
                    case QuestRequirement.StateType.Tracking:
                        GUILayout.Label("Tracking", _tracking, GUILayout.MaxWidth(90));
                        break;
                    case QuestRequirement.StateType.Completed:
                        GUILayout.Label("Completed", _completed, GUILayout.MaxWidth(90));
                        break;
                }
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), GUILayout.MaxWidth(25)))
                {
                    list.DeleteArrayElementAtIndex(i);
                    AssetDatabase.SaveAssets();
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        protected virtual void DrawInheritedProperties() { }
    }
}