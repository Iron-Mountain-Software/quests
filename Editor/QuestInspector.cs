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

        private GUIStyle _header;
        private GUIStyle _h1;
        private GUIStyle _notTracking;
        private GUIStyle _tracking;
        private GUIStyle _completed;
        
        private void OnEnable()
        {
            _quest = (Quest) target;
            _prerequisitesEditor = new ConditionEditor("Prerequisites", _quest,
                newCondition => _quest.Prerequisites = newCondition);
            _onStartActionsEditor = new ScriptableActionsEditor("On Start", _quest, _quest.ActionsOnActivate);
            _onCompleteActionsEditor = new ScriptableActionsEditor("On Complete", _quest, _quest.ActionsOnComplete);
            
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
                DrawRequirements();
                _onCompleteActionsEditor.Draw();
                DrawOtherProperties();
                DrawEditorActionButtons();
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawRequirements()
        {
            GUILayout.Space(10);
            SerializedProperty list = serializedObject.FindProperty("requirements");

            EditorGUILayout.BeginHorizontal(_header,GUILayout.ExpandWidth(true));
            GUILayout.Label("Requirements", _h1, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Add", GUILayout.MaxWidth(125)))
                AddQuestRequirementMenu.Open(_quest);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < list.arraySize; i++)
            {
                QuestRequirement requirement = (QuestRequirement) list.GetArrayElementAtIndex(i).objectReferenceValue;
                if (!requirement) continue;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label((i + 1) + ". " + requirement.name);
                switch (requirement.State)
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
            EditorGUILayout.BeginHorizontal(_header,GUILayout.ExpandWidth(true));
            GUILayout.Label("Other", _h1, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            DrawPropertiesExcluding(serializedObject,
                "m_Script",
                "id",
                "localizedName",
                "description",
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
            EditorGUILayout.BeginHorizontal(_header,GUILayout.ExpandWidth(true));
            GUILayout.Label("Editor Actions", _h1, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Log & Copy Data", GUILayout.MinHeight(25)))
            {
                //EditorGUIUtility.systemCopyBuffer = QuestPrinter.PrintQuest(_quest);
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
