using UnityEditor;

namespace Materiator
{
    public class MateriaTagsWindow : EditorWindow
    {
        private Editor _editor = null;
        private MateriaTags _materiaTags;

        [MenuItem("Tools/Materiator/Tag Editor")]
        public static void OpenWindow()
        {
            GetWindow<MateriaTagsWindow>("Materia Tag Editor");
        }

        private void OnEnable()
        {
            _materiaTags = SystemData.Settings.MateriaTags;
            if (!_editor) Editor.CreateCachedEditor(_materiaTags, null, ref _editor);
        }

        private void OnGUI()
        {
            if (_editor) _editor.OnInspectorGUI();
        }
    }
}