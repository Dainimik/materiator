using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaSetter))]
    public class MateriaSetterEditor : MateriatorEditor
    {
        private MateriaSetter _materiaSetter;

        private Button _reloadButton;

        private void OnEnable()
        {
            _materiaSetter = (MateriaSetter)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaSetter>();

            RegisterButtons();

            return root;
        }

        private void RegisterButtons()
        {
            _reloadButton = root.Q<Button>("ReloadButton");
            _reloadButton.clicked += Reload;
        }

        private void Reload()
        {
            _materiaSetter.Initialize();
        }
    }
}
