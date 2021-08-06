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

        private SerializedProperty _materiaAtlas;

        private ObjectField _materiaAtlasObjectField;
        private VisualElement _atlasIndicator;
        private Button _reloadAtlasButton;

        public AtlasSection(MateriaSetterEditor editor)
        {
            _editor = editor;
            _materiaSetter = editor.MateriaSetter;

            GetProperties();
            BindProperties();
            RegisterCallbacks();

            UpdateIndicator();
        }

        public void OnMateriaAtlasChanged(MateriaAtlas atlas = null)
        {
            if (atlas == null)
            {
                if (_materiaAtlas.objectReferenceValue != null)
                {
                    atlas = _materiaAtlas.objectReferenceValue as MateriaAtlas;
                }
            }

            _materiaAtlas.objectReferenceValue = atlas;
            _editor.serializedObject.ApplyModifiedProperties();

            _editor.MateriaSetter.LoadAtlas(atlas, _materiaSetter.Mesh);

            UpdateIndicator();
        }

        private void UpdateIndicator()
        {
            if (_materiaSetter.MateriaAtlas != null)
            {
                if (_materiaSetter.MateriaAtlas.Material == _materiaSetter.Renderer.sharedMaterial)
                {
                    _atlasIndicator.style.backgroundColor = SystemData.Settings.GUIGreen;
                }
                else
                {
                    _atlasIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
                }
            }
            else
            {
                _atlasIndicator.style.backgroundColor = SystemData.Settings.GUIGray;
            }  
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