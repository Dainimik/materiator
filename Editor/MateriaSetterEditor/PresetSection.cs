using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace Materiator
{
    public class PresetSection
    {
        public Action OnPresetLoaded;

        private MateriaSetterEditor _editor;
        private MateriaSetter _materiaSetter;

        public SerializedProperty MateriaPreset;

        private ObjectField _materiaPresetObjectField;

        private Button _reloadMateriaPresetButton;

        public VisualElement _presetIndicator;

        public PresetSection(MateriaSetterEditor editor)
        {
            _editor = editor;
            _materiaSetter = editor.MateriaSetter;

            GetProperties();
            BindProperties();
            RegisterCallbacks();
            RegisterButtons();

            SetButtonState();
            UpdateIndicator();
        }

        private void SetButtonState()
        {
            if (_materiaPresetObjectField.value == null)
                _reloadMateriaPresetButton.SetEnabled(false);
            else
                _reloadMateriaPresetButton.SetEnabled(true);
        }

        private void UpdateIndicator()
        {
            var value = (MateriaPreset)_materiaPresetObjectField.value;

            if (value != null)
            {
                if (AreMateriasSameAsPreset(value, _materiaSetter.MateriaSlots))
                    _presetIndicator.style.backgroundColor = SystemData.Settings.GUIGreen;
                else
                    _presetIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
            }
            else
                _presetIndicator.style.backgroundColor = SystemData.Settings.GUIGray;
        }

        private void ReloadPreset()
        {
            LoadPreset((MateriaPreset)_materiaPresetObjectField.value);

            OnMateriaPresetChanged();
        }

        private void LoadPreset(MateriaPreset preset)
        {
            List<MateriaSlot> materiaSlots;
            if (_materiaSetter.MateriaSetterData != null)
            {
                materiaSlots = _materiaSetter.MateriaSetterData.MateriaSlots;
            }
            else
            {
                materiaSlots = _materiaSetter.MateriaSlots;
            }
            var same = AreMateriasSameAsPreset(preset, _materiaSetter.MateriaSlots);

            if (!same)
            {
                _editor.SetMateriaSetterDirty(true);

                if (preset != null)
                {
                    _reloadMateriaPresetButton.visible = true;
                    _materiaSetter.LoadPreset(preset);
                }
                else
                {
                    _reloadMateriaPresetButton.visible = false;
                    _materiaSetter.LoadPreset(null);
                }
                _editor.serializedObject.Update();

                _materiaSetter.UpdateColorsOfAllTextures();
            }

            OnPresetLoaded?.Invoke();
        }

        private void OnMateriaPresetChanged()
        {
            UpdateIndicator();
            SetButtonState();
        }

        private bool AreMateriasSameAsPreset(MateriaPreset preset, List<MateriaSlot> materiaSlots)
        {
            var numberOfDifferentMateria = 0;

            if (preset != null)
            {
                for (int i = 0; i < materiaSlots.Count; i++)
                {
                    for (int j = 0; j < preset.MateriaPresetItems.Count; j++)
                    {
                        if (materiaSlots[i].Tag.Name == preset.MateriaPresetItems[j].Tag.Name)
                        {
                            if (materiaSlots[i].Materia != preset.MateriaPresetItems[j].Materia)
                            {
                                numberOfDifferentMateria++;
                            }
                        }
                    }
                }
            }

            var same = false;

            if (numberOfDifferentMateria == 0)
            {
                same = true;
            }

            return same;
        }

        private void RegisterCallbacks()
        {
            _materiaPresetObjectField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                OnMateriaPresetChanged();
            });

            _editor.OnMateriaSetterUpdated += UpdateIndicator;
        }

        private void GetProperties()
        {
            var root = _editor.Root;

            MateriaPreset = _editor.serializedObject.FindProperty("MateriaPreset");

            _materiaPresetObjectField = root.Q<ObjectField>("MateriaPresetObjectField");
            _materiaPresetObjectField.objectType = typeof(MateriaPreset);

            _reloadMateriaPresetButton = root.Q<Button>("ReloadMateriaPresetButton");

            _presetIndicator = root.Q<VisualElement>("PresetIndicator");
        }

        private void BindProperties()
        {
            _materiaPresetObjectField.BindProperty(MateriaPreset);
        }

        private void RegisterButtons()
        {
            _reloadMateriaPresetButton.clicked += ReloadPreset;
        }
    }
}