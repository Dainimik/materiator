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
            _materiaTags = Utils.LoadMateriaTags();
            if (_materiaTags == null) CreateDefaultTagsData();
            if (!_editor) Editor.CreateCachedEditor(_materiaTags, null, ref _editor);
        }

        private void OnGUI()
        {
            if (_editor) _editor.OnInspectorGUI();
        }

        private void CreateDefaultTagsData()
        {
            var editorScriptPath = AssetUtils.GetEditorScriptDirectory(this);
            AssetUtils.CheckDirAndCreate(editorScriptPath, "Resources");
            var path = editorScriptPath + "/Resources";
            _materiaTags = AssetUtils.CreateScriptableObjectAsset<MateriaTags>(path, "MateriaTags");

            _materiaTags.MateriaTagsList.Add("-");
            _materiaTags.MateriaTagsList.Add("Metal");
            _materiaTags.MateriaTagsList.Add("Plastic");

            return;
        }
    }
}