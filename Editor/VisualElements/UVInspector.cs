using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    public enum UVDisplayMode
    {
        BaseColor,
        MetallicSpecularGlossSmoothness,
        EmissionColor
    }
    public class UVInspector : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UVInspector> { }

        private VisualElement _root;
        private VisualTreeAsset _tree;
        private MateriaSetter _materiaSetter;
        private VisualElement _parentRoot;

        private VisualElement _uvInspectorContainer;
        private VisualElement _uvIslandContainer;
        public EnumField DisplayModeEnumField;

        public UVInspector() {}
        public UVInspector(MateriaSetter materiaSetter, VisualElement root)
        {
            _parentRoot = root;
            _root = new VisualElement();
            _tree = Resources.Load<VisualTreeAsset>("VisualElements/UVInspector");
            _tree.CloneTree(_root);

            _materiaSetter = materiaSetter;

            BindControls();
            RegisterCallbacks();

            DrawUVInspector(false);

            Add(_root);
            _uvInspectorContainer.Add(_root);
        }

        public void DrawUVInspector(bool redraw)
        {
            if (redraw)
            {
                _uvIslandContainer.Clear();
            }

            var rects = _materiaSetter.Rects;
            var size = Mathf.Sqrt(rects.Length);
            Color borderColor;

            for (int i = 0, y = 0; y < size; y++)
            {
                var horizontalContainer = new VisualElement();
                horizontalContainer.style.flexGrow = 1;
                horizontalContainer.style.flexShrink = 0;
                horizontalContainer.style.flexDirection = FlexDirection.Row;
                _uvIslandContainer.Add(horizontalContainer);

                for (int x = 0; x < size; x++, i++)
                {
                    var item = new VisualElement();
                    item.name = "UVGridItem";
                    item.styleSheets.Add(Resources.Load<StyleSheet>("Materiator"));

                    if (_materiaSetter.FilteredRects.ContainsKey(i))
                    {
                        Color color = SystemData.Settings.DefaultMateria.BaseColor;
                        switch (DisplayModeEnumField.value)
                        {
                            case UVDisplayMode.BaseColor:
                                color = _materiaSetter.MateriaSlots.Where(ms => ms.ID == i).First().Materia.BaseColor;
                                break;
                            case UVDisplayMode.MetallicSpecularGlossSmoothness:
                                var metallic = _materiaSetter.MateriaSlots.Where(ms => ms.ID == i).First().Materia.Metallic;
                                var metallicColor = new Color(metallic, metallic, metallic, 1f);
                                color = metallicColor;
                                break;
                            case UVDisplayMode.EmissionColor:
                                color = _materiaSetter.MateriaSlots.Where(ms => ms.ID == i).First().Materia.EmissionColor;
                                break;
                        }

                        borderColor = Color.green;
                        item.style.backgroundColor = color;
                        item.style.borderTopColor = borderColor;
                        item.style.borderBottomColor = borderColor;
                        item.style.borderLeftColor = borderColor;
                        item.style.borderRightColor = borderColor;
                    }
                    else
                    {
                        borderColor = Color.red;
                        item.style.borderTopColor = borderColor;
                        item.style.borderBottomColor = borderColor;
                        item.style.borderLeftColor = borderColor;
                        item.style.borderRightColor = borderColor;
                    }

                    horizontalContainer.Add(item);

                    var label = new Label();
                    label.text = (i + 1).ToString();
                    item.Add(label);
                }
            }
        }

        private void BindControls()
        {
            _uvInspectorContainer = _parentRoot.Q<VisualElement>("UVInspectorContainer");
            _uvIslandContainer = _root.Q<VisualElement>("UVIslandContainer");

            DisplayModeEnumField = _root.Q<EnumField>("UVDisplayMode");
            DisplayModeEnumField.Init(UVDisplayMode.BaseColor);
        }

        private void RegisterCallbacks()
        {
            DisplayModeEnumField.RegisterCallback<ChangeEvent<System.Enum>>(e =>
            {
                DisplayModeEnumField.value = e.newValue;
                DrawUVInspector(true);
            });
        }
    }
}