using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaTags))]
    public class MateriaTagsEditor : MateriatorEditor
    {
        private MateriaTags _materiaTags;

        //private ReorderableList _tagsList;

        private void OnEnable()
        {
            _materiaTags = (MateriaTags)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaTags>();

            IMGUIContainer materiaTagsReorderableList = new IMGUIContainer(() => DrawDefaultInspector());
            root.Add(materiaTagsReorderableList);

            return root;
        }
    }
}