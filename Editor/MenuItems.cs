using UnityEditor;

namespace IronMountain.Quests.Editor
{
    public static class MenuItems
    {
        [MenuItem("Iron Mountain/Quests Window", priority = 1)]
        static void OpenEditorWindow()
        {
            QuestsEditorWindow.Open();
        }
    }
}