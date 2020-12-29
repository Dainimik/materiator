using UnityEditor;

namespace Materiator
{
    public static class MenuItems
    {
        private const string MENU_NAME = "Tools/Materiator/";

        [MenuItem(MENU_NAME + "Settings")]
        private static void Settings()
        {
            MateriatorSettingsWindow.OpenWindow();
        }
    }
}