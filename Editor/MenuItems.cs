using UnityEditor;

namespace IronMountain.Quests.Editor
{
    public static class MenuItems
    {
        [MenuItem("Iron Mountain/Quests/Open Editor Window", priority = 1)]
        static void OpenEditorWindow()
        {
            QuestsEditorWindow.Open();
        }
    }
}