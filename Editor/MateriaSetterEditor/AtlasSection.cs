using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class AtlasSection
    {
        private MateriaSetterEditor _editor;
        private MateriaSetter _materiaSetter;

        public SerializedProperty _materiaAtlas;

        private ObjectField _materiaAtlasObjectField;

        public VisualElement _atlasIndicator;

        private Button _reloadAtlasButton;

        public AtlasSection(MateriaSetterEditor editor)
        {
            _editor = editor;
            _materiaSetter = editor.MateriaSetter;

            GetProperties();
            BindProperties();
            RegisterButtons();
            RegisterCallbacks();
            
            OnMateriaAtlasChanged(_materiaAtlas.objectReferenceValue as MateriaAtlas);
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
            _materiaAtlas.objectReferenceValue = atlas;
            _editor.serializedObject.ApplyModifiedProperties();

            _editor.MateriaSetter.LoadAtlas(atlas);

            UpdateIndicator();
        }

        private void GetProperties()
        {
            var root = _editor.Root;

            _materiaAtlas = _editor.serializedObject.FindProperty("MateriaAtlas");

            _materiaAtlasObjectField = root.Q<ObjectField>("MateriaAtlasObjectField");
            _materiaAtlasObjectField.objectType = typeof(MateriaAtlas);

            _atlasIndicator = root.Q<VisualElement>("AtlasIndicator");
            _reloadAtlasButton = root.Q<Button>("ReloadAtlasButton");
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
            _reloadAtlasButton.clicked += () => OnMateriaAtlasChanged((MateriaAtlas)_materiaAtlas.objectReferenceValue);

            _materiaAtlasObjectField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                OnMateriaAtlasChanged((MateriaAtlas)e.newValue);
            });

            _editor.OnTagChanged += () => OnMateriaAtlasChanged(_materiaAtlas.objectReferenceValue as MateriaAtlas);
        }
    }
}