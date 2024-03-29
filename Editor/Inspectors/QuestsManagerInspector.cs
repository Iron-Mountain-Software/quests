using UnityEditor;

namespace IronMountain.Quests.Editor.Inspectors
{
    [CustomEditor(typeof(QuestsManager), true)]
    public class SceneManagerInspector : UnityEditor.Editor
    {
        private UnityEditor.Editor _databaseInspector;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Database database = serializedObject.FindProperty("database").objectReferenceValue as Database;
            if (database)
            {
                CreateCachedEditor(database, null, ref _databaseInspector);
                _databaseInspector.OnInspectorGUI();
            }
            else _databaseInspector = null;
        }
    }
}