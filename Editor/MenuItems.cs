﻿using UnityEditor;

namespace Materiator
{
    public static class MenuItems
    {
        private const string MENU_NAME = "Tools/Materiator/";

        [MenuItem(MENU_NAME + "Atlas Editor")]
        private static void AtlasEditor()
        {
            MateriaAtlasEditorWindow.OpenWindow();
        }

        [MenuItem(MENU_NAME + "Tag Editor")]
        private static void TagEditor()
        {
            MateriaTagsWindow.OpenWindow();
        }

        [MenuItem(MENU_NAME + "Settings")]
        private static void Settings()
        {
            MateriatorSettingsWindow.OpenWindow();
        }
    }
}