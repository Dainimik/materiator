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
        public VisualElement MaterialDataIcon;
        public Label MaterialDataLabel;

        public Button FocusListEntryButton;
        public Button RemoveListEntryButton;

        public AtlasEditorListEntry(string index, Texture2D prefabIcon, string prefabName, Texture2D materiaSetterIcon, string materiaSetterName, Texture2D materialDataIcon, string materialDataName)
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
            MaterialDataIcon.style.backgroundImage = materialDataIcon;
            MaterialDataLabel.text = materialDataName;

            Add(_root);
        }

        private void BindControls()
        {
            Index = _root.Q<Label>("IndexLabel");
            PrefabIcon = _root.Q<VisualElement>("PrefabIcon");
            PrefabLabel = _root.Q<Label>("PrefabLabel");
            MateriaSetterIcon = _root.Q<VisualElement>("MateriaSetterIcon");
            MateriaSetterLabel = _root.Q<Label>("MateriaSetterLabel");
            MaterialDataIcon = _root.Q<VisualElement>("MaterialDataIcon");
            MaterialDataLabel = _root.Q<Label>("MaterialDataLabel");

            FocusListEntryButton = _root.Q<Button>("FocusListEntryButton");
            RemoveListEntryButton = _root.Q<Button>("RemoveListEntryButton");
        }
    }
}