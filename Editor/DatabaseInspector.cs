using UnityEditor;
using UnityEngine;

namespace IronMountain.Quests.Editor
{
    [CustomEditor(typeof(Database), true)]
    public class DatabaseInspector : UnityEditor.Editor
    {
        private Database _database;

        private void OnEnable()
        {
            _database = (Database) target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rebuild Quest List"))
            {
                RebuildList();
            }
            if (GUILayout.Button("Sort Quest List"))
            {
                _database.SortList();
            }
            if (GUILayout.Button("Rebuild Dictionary"))
            {
                _database.RebuildDictionary();
            }
            if (GUILayout.Button("Log & Copy Data"))
            {
                string data = _database.ToString();
                EditorGUIUtility.systemCopyBuffer = data;
                Debug.Log(data);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            DrawDefaultInspector();
            
            serializedObject.ApplyModifiedProperties();
        }

        private void RebuildList()
        {
            _database.Quests.Clear();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(Quest)}");
            for( int i = 0; i < guids.Length; i++ )
            {
                string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
                Quest quest = AssetDatabase.LoadAssetAtPath<Quest>( assetPath );
                if (quest) _database.Quests.Add(quest);
            }
            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}