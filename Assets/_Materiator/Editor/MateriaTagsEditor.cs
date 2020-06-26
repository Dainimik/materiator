using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaTags))]
    public class MateriaTagEditor : MateriatorEditor
    {
        private MateriaTags _materiaTags;

        private Button _newTagButton;
        private Button _deleteTagButton;

        private void OnEnable()
        {
            _materiaTags = (MateriaTags)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaTags>();

            SetUpButtons();
            SyncCategoryUI();

            return root;
        }

        private void SetUpButtons()
        {
            _newTagButton = root.Q<Button>("NewTagButton");
            _deleteTagButton = root.Q<Button>("DeleteTagButton");

            _newTagButton.clicked += AddTag;

            if (_deleteTagButton != null)
            {
                _deleteTagButton.clicked += DeleteTag;
            }
        }

        private void AddTag()
        {
            //Undo.RecordObject(targetMonster, "Add Monster Attack");
            _materiaTags.MateriaTagDictionary.Add(_materiaTags.MateriaTagDictionary.Count + 1, "aaa");
            var newTag = new MateriaTagElement();
            root.Add(newTag);
        }

        private void DeleteTag()
        {
            //_materiaPreset.MateriaCategories.Remove(_materiaPreset.MateriaCategories.Count + 1, "aaa");
        }

        private void SyncCategoryUI()
        {
            //root.Clear();
            for (var i = 0; i < _materiaTags.MateriaTagDictionary.Count; i++)
            {
                var newTag = new MateriaTagElement();
                root.Add(newTag);
            }
        }
    }
}