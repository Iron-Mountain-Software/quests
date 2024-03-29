using IronMountain.Conditions.Editor;
using IronMountain.ScriptableActions.Editor;
using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    public abstract class StoryEventEditor : StyledInspector
    {
        private StoryEvent _storyEvent;
        private ConditionEditor _prerequisitesEditor;
        private ConditionEditor _toCompleteEditor;
        private ConditionEditor _toFailEditor;
        private ScriptableActionsEditor _onActivateActionsEditor;
        private ScriptableActionsEditor _onCompleteActionsEditor;
        private ScriptableActionsEditor _onFailActionsEditor;
        
        public bool localize;

        protected abstract void DrawMainSection();
        
        protected override void OnEnable()
        {
            base.OnEnable();
            if (target) _storyEvent = (StoryEvent) target;
            _prerequisitesEditor = new ConditionEditor("Prerequisites", _storyEvent,
                newCondition =>
                {
                    _storyEvent.prerequisites = newCondition;
                    _storyEvent.OnValidate();
                    EditorUtility.SetDirty(_storyEvent);
                    AssetDatabase.SaveAssets();
                });
            _toCompleteEditor = new ConditionEditor("To Complete", _storyEvent,
                newCondition =>
                {
                    _storyEvent.completionCondition = newCondition;
                    _storyEvent.OnValidate();
                    EditorUtility.SetDirty(_storyEvent);
                    AssetDatabase.SaveAssets();
                });
            _toFailEditor = new ConditionEditor("To Fail", _storyEvent,
                newCondition =>
                {
                    _storyEvent.failCondition = newCondition;
                    _storyEvent.OnValidate();
                    EditorUtility.SetDirty(_storyEvent);
                    AssetDatabase.SaveAssets();
                });
            _onActivateActionsEditor = new ScriptableActionsEditor("On Activate", _storyEvent, _storyEvent ? _storyEvent.actionsOnActivate : null);
            _onCompleteActionsEditor = new ScriptableActionsEditor("On Complete", _storyEvent, _storyEvent ? _storyEvent.actionsOnComplete : null);
            _onFailActionsEditor = new ScriptableActionsEditor("On Fail", _storyEvent, _storyEvent ? _storyEvent.actionsOnFail : null);
        }
        
        public override void OnInspectorGUI()
        {
            DrawMainSection();
            serializedObject.ApplyModifiedProperties();
            
            _prerequisitesEditor.Draw(ref _storyEvent.prerequisites);
            _onActivateActionsEditor.Draw();

            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical();
            _toCompleteEditor.Draw(ref _storyEvent.completionCondition);
            _onCompleteActionsEditor.Draw();
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical(GUILayout.Width(10));
            GUILayout.Space(10);
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical();
            _toFailEditor.Draw(ref _storyEvent.failCondition);
            _onFailActionsEditor.Draw();
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
            
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawStateControls(bool disabled)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(126));
            StoryEvent.StateType state = _storyEvent.State;
            switch (state)
            {
                case StoryEvent.StateType.Inactive:
                    GUILayout.Label("Inactive", Inactive, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(40));
                    break;
                case StoryEvent.StateType.Active:
                    GUILayout.Label("Active", Active, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(40));
                    break;
                case StoryEvent.StateType.Complete:
                    GUILayout.Label("Complete", Complete, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(40));
                    break;
                case StoryEvent.StateType.Failed:
                    GUILayout.Label("Failed", Fail, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(40));
                    break;
            }

            EditorGUI.BeginDisabledGroup(disabled);

            if (state is StoryEvent.StateType.Active)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Fail", GUILayout.ExpandHeight(true), GUILayout.MaxHeight(30)))
                    _storyEvent.Fail();
                if (GUILayout.Button("Complete", GUILayout.ExpandHeight(true), GUILayout.MaxHeight(30)))
                    _storyEvent.Complete();
                GUILayout.EndHorizontal();
            }
            
            if (state is StoryEvent.StateType.Inactive && GUILayout.Button("Activate", GUILayout.ExpandHeight(true), GUILayout.MaxHeight(30)))
            {
                _storyEvent.Activate();
            }
            if (state is not StoryEvent.StateType.Inactive && GUILayout.Button("Restart", GUILayout.ExpandHeight(true), GUILayout.MaxHeight(30)))
            {
                _storyEvent.Restart();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(8);
            
            if (GUILayout.Button("Log & Copy Data", GUILayout.ExpandHeight(true), GUILayout.MaxHeight(40)))
            {
                string documentation = _storyEvent.GetDocumentation();
                Debug.Log(documentation);
                EditorGUIUtility.systemCopyBuffer = documentation;
            }
            EditorGUILayout.EndVertical();
        }
    }
}