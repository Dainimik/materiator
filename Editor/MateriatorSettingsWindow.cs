using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class MateriatorSettingsWindow : EditorWindow
    {
        private Editor _editor = null;
        private MateriatorSettings _settings;

        [MenuItem("Tools/Materiator/Settings")]
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