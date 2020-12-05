using UnityEditor;

namespace Materiator
{
    public class MateriatorSettingsWindow : EditorWindow
    {
        private Editor _editor = null;
        private MateriatorSettings _settings;

        public static void OpenWindow()
        {
            GetWindow<MateriatorSettingsWindow>("Materiator Settings");
        }

        private void OnEnable()
        {
            _settings = SystemData.Settings;
            if (!_editor) Editor.CreateCachedEditor(_settings, null, ref _editor);
        }

        private void OnGUI()
        {
            if (_editor) _editor.OnInspectorGUI();
        }
    }
}