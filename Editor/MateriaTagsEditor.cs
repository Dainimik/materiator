using UnityEditor;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaTagCollection))]
    public class MateriaTagsEditor : MateriatorEditor
    {
        private MateriaTagCollection _materiaTags;

        private void OnEnable()
        {
            _materiaTags = (MateriaTagCollection)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaTagCollection>();

            IMGUIContainer materiaTagsReorderableListContainer = new IMGUIContainer(() => ExecuteIMGUI());
            root.Add(materiaTagsReorderableListContainer);

            return root;
        }

        private void ExecuteIMGUI()
        {
            DrawDefaultInspector();
        }

        protected override void SetUpView()
        {
            //
        }

        protected override void GetProperties()
        {
            //
        }

        protected override void BindProperties()
        {
            //
        }

        protected override void RegisterCallbacks()
        {
            //
        }
    }
}