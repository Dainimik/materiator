using UnityEditor;

namespace Materiator
{
    public class MateriatorSettingsWindow : EditorWindow
    {
        [MenuItem("Tools/Materiator/Settings")]
        public static void OpenWindow()
        {
            GetWindow<MateriatorSettingsWindow>("Materiator Settings");
        }
    }
}