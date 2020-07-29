using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;

namespace Materiator
{
    public class OutputSection
    {
        private MateriaSetterEditor _editor;
        private MateriaSetter _materiaSetter;

        private VisualElement _outputIndicator;

        private Button _overwriteMateriaSetterData;
        private Button _saveAsNewMateriaSetterData;

        public OutputSection(MateriaSetterEditor editor)
        {
            _editor = editor;
            _materiaSetter = editor.MateriaSetter;

            GetProperties();
            BindProperties();
            RegisterCallbacks();
            RegisterButtons();

            SetButtonStates();
            UpdateIndicator(_editor.IsDirty.boolValue);
        }

        private void UpdateIndicator(bool value)
        {
            if (value == true)
                _outputIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
            else
                _outputIndicator.style.backgroundColor = SystemData.Settings.GUIGreen;
        }

        private void SetButtonStates()
        {
            if (_editor.DataSection.MateriaSetterData.objectReferenceValue == null)
                _overwriteMateriaSetterData.SetEnabled(false);
            else
                _overwriteMateriaSetterData.SetEnabled(true);
        }

        private void OverwriteMateriaSetterData()
        {
            if (EditorUtility.DisplayDialog("Overwrite current data?", "Are you sure you want to overwrite " + _editor.DataSection.MateriaSetterData.objectReferenceValue.name + " with current settings?", "Yes", "No"))
            {
                MateriaDataFactory.WriteAssetsToDisk(_editor, AssetDatabase.GetAssetPath(_materiaSetter.MateriaSetterData), SystemData.Settings.PackAssets);

            }
        }
        private void SaveAsNewMateriaSetterData()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save data", _materiaSetter.gameObject.name, "asset", "asset");
            if (path.Length != 0)
            {
                MateriaDataFactory.WriteAssetsToDisk(_editor, path, SystemData.Settings.PackAssets);
            }
        }

        private void RegisterCallbacks()
        {
            _editor.OnDirtyChanged += UpdateIndicator; // This event is not deregistered anywhere
        }

        private void GetProperties()
        {
            var root = _editor.Root;

            _overwriteMateriaSetterData = root.Q<Button>("OverwriteMateriaSetterDataButton");
            _saveAsNewMateriaSetterData = root.Q<Button>("SaveAsNewMateriaSetterDataButton");

            _outputIndicator = root.Q<VisualElement>("OutputIndicator");
        }

        private void BindProperties()
        {
            
        }

        private void RegisterButtons()
        {
            _overwriteMateriaSetterData.clicked += OverwriteMateriaSetterData;
            _saveAsNewMateriaSetterData.clicked += SaveAsNewMateriaSetterData;
        }
    }
}