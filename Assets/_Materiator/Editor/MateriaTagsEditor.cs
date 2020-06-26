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

            //_tagsList = new ReorderableList(serializedObject.FindProperty("MateriaTagList"), true, true, true);
            TagsListUI();
            IMGUIContainer materiaTagsReorderableList = new IMGUIContainer(() => DrawDefaultInspector());
            //IMGUIContainer materiaTagsReorderableList = new IMGUIContainer(() => DrawTagsList());
            root.Add(materiaTagsReorderableList);

            return root;
        }

        private void DrawTagsList()
        {
            serializedObject.Update();
            //_tagsList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void TagsListUI()
        {

        }
    }
}