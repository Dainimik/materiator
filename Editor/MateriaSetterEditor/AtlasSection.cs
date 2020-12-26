using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;

namespace Materiator
{
    public class AtlasSection
    {
        private MateriaSetterEditor _editor;
        private MateriaSetter _materiaSetter;

        private SerializedProperty _materiaAtlas;

        private ObjectField _materiaAtlasObjectField;

        public VisualElement _atlasIndicator;

        public AtlasSection(MateriaSetterEditor editor)
        {
            _editor = editor;
            _materiaSetter = editor.MateriaSetter;

            GetProperties();
            BindProperties();
            RegisterButtons();
            RegisterCallbacks();

            UpdateIndicator();

            OnMateriaAtlasChanged();
        }

        private void UpdateIndicator()
        {
            if (_materiaSetter.MateriaAtlas != null)
                _atlasIndicator.style.backgroundColor = SystemData.Settings.GUIGreen;
            else
                _atlasIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
        }

        private void OnMateriaAtlasChanged(MateriaAtlas atlas = null)
        {
            UpdateIndicator();

            if (atlas != null)
            {
                _editor.MateriaSetter.LoadAtlas(atlas);

                _editor.serializedObject.Update();
            }
        }

        private void GetProperties()
        {
            var root = _editor.Root;

            _materiaAtlas = _editor.serializedObject.FindProperty("MateriaAtlas");

            _materiaAtlasObjectField = root.Q<ObjectField>("MateriaAtlasObjectField");
            _materiaAtlasObjectField.objectType = typeof(MateriaAtlas);

            _atlasIndicator = root.Q<VisualElement>("AtlasIndicator");
        }

        private void BindProperties()
        {
            _materiaAtlasObjectField.BindProperty(_materiaAtlas);
        }

        private void RegisterButtons()
        {
            
        }

        private void RegisterCallbacks()
        {
            _materiaAtlasObjectField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                OnMateriaAtlasChanged((MateriaAtlas)e.newValue);
            });

            _editor.OnMateriaSetterUpdated += UpdateIndicator;
        }
    }
}