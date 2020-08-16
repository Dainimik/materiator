using UnityEngine;
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

        private Button _switchEditModeButton;

        public VisualElement _atlasIndicator;

        public AtlasSection(MateriaSetterEditor editor)
        {
            _editor = editor;
            _materiaSetter = editor.MateriaSetter;

            GetProperties();
            BindProperties();
            RegisterButtons();

            UpdateIndicator();
            SetButtonState();

            OnMateriaAtlasChanged();
        }

        private void UpdateIndicator()
        {
            if (_materiaSetter.MateriaSetterData != null)
            {
                if (_materiaSetter.MateriaAtlas != null)
                    _atlasIndicator.style.backgroundColor = SystemData.Settings.GUIGreen;
                else
                    _atlasIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
            }
            else
                _atlasIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
        }

        private void SetButtonState()
        {
            if (_materiaSetter.MateriaAtlas == null)
                _switchEditModeButton.SetEnabled(false);
            else
                _switchEditModeButton.SetEnabled(true);
        }

        private void SwitchEditMode()
        {
            EditorUtility.SetDirty(_editor.MateriaSetter);
            if (_materiaSetter.EditMode == EditMode.Native)
                LoadAtlas(_materiaSetter.MateriaSetterData.MateriaAtlas);
            else if (_materiaSetter.EditMode == EditMode.Atlas)
                UnloadAtlas();

            OnMateriaAtlasChanged();
        }

        public void LoadAtlas(MateriaAtlas atlas)
        {
            if (atlas != null)
            {
                _materiaSetter.MateriaSlots = _materiaSetter.MateriaSetterData.MateriaSlots;

                _materiaSetter.LoadAtlas(atlas);
            }

            _editor.SetMateriaSetterDirty(false);
        }

        public void UnloadAtlas()
        {
            _materiaSetter.UnloadAtlas();
        }

        private void OnMateriaAtlasChanged()
        {
            UpdateIndicator();

            SetButtonState();

            _switchEditModeButton.text = _editor.EditMode.enumNames[_editor.EditMode.enumValueIndex] + " Mode";
        }

        private void GetProperties()
        {
            var root = _editor.Root;

            _materiaAtlas = _editor.serializedObject.FindProperty("MateriaAtlas");

            _materiaAtlasObjectField = root.Q<ObjectField>("MateriaAtlasObjectField");
            _materiaAtlasObjectField.objectType = typeof(MateriaAtlas);
            _materiaAtlasObjectField.SetEnabled(false);

            _switchEditModeButton = root.Q<Button>("SwitchEditMode");

            _atlasIndicator = root.Q<VisualElement>("AtlasIndicator");
        }

        private void BindProperties()
        {
            _materiaAtlasObjectField.BindProperty(_materiaAtlas);
        }

        private void RegisterButtons()
        {
            _switchEditModeButton.clicked += SwitchEditMode;
        }
    }
}