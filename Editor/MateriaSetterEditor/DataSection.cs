using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;

namespace Materiator
{
    public class DataSection
    {
        private MateriaSetterEditor _editor;
        private MateriaSetter _materiaSetter;

        public SerializedProperty MateriaSetterData;
        public SerializedProperty MaterialData;

        private ObjectField _materiaSetterDataObjectField;
        private ObjectField _materialDataObjectField;

        private Label _currentShaderLabel;

        private Button _reloadMateriaSetterDataButton;
        private Button _newMateriaSetterDataButton;

        public VisualElement _dataIndicator;

        public DataSection(MateriaSetterEditor editor)
        {
            _editor = editor;
            _materiaSetter = editor.MateriaSetter;

            GetProperties();
            BindProperties();
            RegisterCallbacks();
            RegisterButtons();

            SetReloadDataButtonState((MateriaSetterData)_materiaSetterDataObjectField.value);
            SetCurrentShaderLabel(_materiaSetter.MaterialData.ShaderData.Shader.name);
            UpdateIndicator(_editor.IsDirty.boolValue);

            if (MateriaSetterData.objectReferenceValue == null)
            {
                _editor.SetMateriaSetterDirty(true);
            }
        }

        private void SetReloadDataButtonState(MateriaSetterData data)
        {
            if (data == null)
                _reloadMateriaSetterDataButton.SetEnabled(false);
            else
                _reloadMateriaSetterDataButton.SetEnabled(true);
        }

        private void SetCurrentShaderLabel(string text)
        {
            if (_currentShaderLabel != null)
                _currentShaderLabel.text = text;
        }

        private void UpdateIndicator(bool value)
        {
            if (value == true)
                _dataIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
            else
                _dataIndicator.style.backgroundColor = SystemData.Settings.GUIGreen;
        }

        private void CreateNewData()
        {
            if (_editor.EditMode.enumValueIndex == 1)
                _materiaSetter.UnloadAtlas();

            _editor.ResetMateriaSetter();
        }

        public void ReloadData(MateriaSetterData data)
        {
            OnMateriaSetterDataChanged(data);
        }

        private void OnMateriaSetterDataChanged(MateriaSetterData data)
        {
            if (data != null)
            {
                MateriaSetterData.objectReferenceValue = data;
                _editor.serializedObject.ApplyModifiedProperties();

                Utils.ShallowCopyFields(data, _materiaSetter);

                if (_editor.EditMode.enumValueIndex == 0)
                {
                    _materiaSetter.Mesh = data.NativeMesh;
                }
                else if (_editor.EditMode.enumValueIndex == 1)
                {
                    if (data.MateriaAtlas != null)
                    {
                        _editor.LoadAtlas(data.MateriaAtlas);
                        _materiaSetter.AnalyzeMesh(); // not sure why this is needed here
                    }
                    else
                    {
                        _editor.UnloadAtlas();
                    }
                }

                _editor.serializedObject.Update();
                _materiaSetter.UpdateRenderer();
            }
            else
            {
                _materiaSetterDataObjectField.value = MateriaSetterData.objectReferenceValue;
            }

            _editor.SetMateriaSetterDirty(false);

            SetReloadDataButtonState(data);
        }

        private void OnMaterialDataChanged(MaterialData materialData)
        {

            if (materialData.Material.shader != materialData.ShaderData.Shader)
                materialData.Material.shader = materialData.ShaderData.Shader;

            SetCurrentShaderLabel(materialData.ShaderData.Shader.name);

            _editor.SetMateriaSetterDirty(true);

            _editor.Material.objectReferenceValue = Material.Instantiate(materialData.Material);

            _editor.serializedObject.ApplyModifiedProperties();

            _materiaSetter.Material.name = _materiaSetter.gameObject.name;
            _materiaSetter.Material.shader = materialData.ShaderData.Shader;

            _materiaSetter.SetTextures();
            _materiaSetter.UpdateRenderer(false);
        }

        private void RegisterCallbacks()
        {
            _materiaSetterDataObjectField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                OnMateriaSetterDataChanged((MateriaSetterData)e.newValue);
            });

            _materialDataObjectField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                OnMaterialDataChanged((MaterialData)e.newValue);
            });

            _editor.OnDirtyChanged += UpdateIndicator; // This event is not deregistered anywhere
        }

        private void GetProperties()
        {
            var root = _editor.Root;

            MateriaSetterData = _editor.serializedObject.FindProperty("MateriaSetterData");
            MaterialData = _editor.serializedObject.FindProperty("MaterialData");

            _materiaSetterDataObjectField = root.Q<ObjectField>("MateriaSetterDataObjectField");
            _materiaSetterDataObjectField.objectType = typeof(MateriaSetterData);
            _materialDataObjectField = root.Q<ObjectField>("MaterialDataObjectField");
            _materialDataObjectField.objectType = typeof(MaterialData);

            _newMateriaSetterDataButton = root.Q<Button>("NewMateriaSetterDataButton");
            _reloadMateriaSetterDataButton = root.Q<Button>("ReloadMateriaSetterDataButton");

            _dataIndicator = root.Q<VisualElement>("DataIndicator");

            _currentShaderLabel = root.Q<Label>("CurrentShaderLabel");
        }

        private void BindProperties()
        {
            _materiaSetterDataObjectField.BindProperty(MateriaSetterData);
            _materialDataObjectField.BindProperty(MaterialData);
        }

        private void RegisterButtons()
        {
            _newMateriaSetterDataButton.clicked += CreateNewData;
            _reloadMateriaSetterDataButton.clicked += () => { ReloadData((MateriaSetterData)_materiaSetterDataObjectField.value); };
        }
    }
}