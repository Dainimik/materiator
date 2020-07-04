using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    public class AtlasEditorListEntry : VisualElement
    {
        private VisualElement _root;
        private VisualTreeAsset _tree;

        public Label Index;
        public VisualElement PrefabIcon;
        public Label PrefabLabel;
        public VisualElement MateriaSetterIcon;
        public Label MateriaSetterLabel;

        public Button FocusListEntryButton;
        public Button RemoveListEntryButton;

        public AtlasEditorListEntry(string index, Texture2D prefabIcon, string prefabName, Texture2D materiaSetterIcon, string materiaSetterName)
        {
            _root = new VisualElement();
            _tree = Resources.Load<VisualTreeAsset>("VisualElements/AtlasEditorListEntry");
            _tree.CloneTree(_root);

            BindControls();

            Index.text = index;
            PrefabIcon.style.backgroundImage = prefabIcon;
            PrefabLabel.text = prefabName;
            MateriaSetterIcon.style.backgroundImage = materiaSetterIcon;
            MateriaSetterLabel.text = materiaSetterName;

            Add(_root);
        }

        private void BindControls()
        {
            Index = _root.Q<Label>("IndexLabel");
            PrefabIcon = _root.Q<VisualElement>("PrefabIcon");
            PrefabLabel = _root.Q<Label>("PrefabLabel");
            MateriaSetterIcon = _root.Q<VisualElement>("MateriaSetterIcon");
            MateriaSetterLabel = _root.Q<Label>("MateriaSetterLabel");

            FocusListEntryButton = _root.Q<Button>("FocusListEntryButton");
            RemoveListEntryButton = _root.Q<Button>("RemoveListEntryButton");
        }
    }
}