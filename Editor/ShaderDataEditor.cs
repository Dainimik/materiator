using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Materiator
{
    [CustomEditor(typeof(ShaderData))]
    public class ShaderDataEditor : MateriatorEditor
    {
        private ShaderData _shaderData;
        private List<string> _propertiesToRemove = new List<string>();

        private IMGUIContainer _IMGUIContainer;

        private SerializedProperty _shader;

        private ObjectField _shaderObjectField;
        private Button _updateShaderPropertiesButton;

        private List<KeyValuePair<int, ShaderProperty>> _newProperties;



        private ListView _availableShaderPropertiesListView;
        private ListView _selectedShaderPropertiesListView;

        private SerializedProperty _availableShaderProperties;
        private SerializedProperty _selectedShaderProperties;

        private Button _addPropertyToSelected;
        private Button _removePropertyFromSelected;

        private void OnEnable()
        {
            _shaderData = (ShaderData)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<ShaderData>();

            IMGUIContainer defaultInspector = new IMGUIContainer(() => IMGUI());
            _IMGUIContainer.Add(defaultInspector);

            DrawShaderPropertiesListView(_availableShaderPropertiesListView, _selectedShaderPropertiesListView, _shaderData.AvailableShaderProperties, _shaderData.SelectedShaderProperties);
            DrawShaderPropertiesListView(_selectedShaderPropertiesListView, _availableShaderPropertiesListView, _shaderData.SelectedShaderProperties, _shaderData.AvailableShaderProperties);

            return root;
        }

        private void IMGUI()
        {
            base.DrawDefaultInspector();
        }

        private void DrawShaderPropertiesListView(ListView listView, ListView targetListView, List<string> list, List<string> targetList)
        {
            // The "makeItem" function will be called as needed
            // when the ListView needs more items to render
            Func<VisualElement> makeItem = () => new Label();

            // As the user scrolls through the list, the ListView object
            // will recycle elements created by the "makeItem"
            // and invoke the "bindItem" callback to associate
            // the element with the matching data item (specified as an index in the list)
            Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = list[i];
            listView.makeItem = makeItem;
            listView.bindItem = bindItem;
            
            listView.itemsSource = list;
            listView.selectionType = SelectionType.Multiple;

            // Callback invoked when the user double clicks an item
            listView.onItemsChosen += (items) =>
            {
                foreach (var item in items)
                {
                    list.Remove(item.ToString());
                    targetList.Add(item.ToString());
                }

                GetShaderProperties();

                listView.Refresh();
                targetListView.Refresh();
            };
        }

        private void GetShaderProperties()
        {
            var propertyCount = ShaderUtil.GetPropertyCount(_shaderData.Shader);

            _shaderData.AvailableShaderProperties.Clear();

            for (int i = 0; i < propertyCount; i++)
            {
                var prop = ShaderUtil.GetPropertyName(_shaderData.Shader, i) + " - " + ShaderUtil.GetPropertyDescription(_shaderData.Shader, i);

                if (!_shaderData.SelectedShaderProperties.Contains(prop))
                    _shaderData.AvailableShaderProperties.Add(prop);
            }
        }

        private void AddPropertyToSelected(IEnumerable items, List<string> source, List<string> dest)
        {
            foreach (var item in items)
            {
                source.Remove(item.ToString());
                dest.Add(item.ToString());
            }

            _availableShaderPropertiesListView.Refresh();
            _selectedShaderPropertiesListView.Refresh();
        }

        private void UpdateShaderProperties()
        {
            GetShaderProperties();

            var shaderDataProperties = _shaderData.Properties;
            var keywords = _shaderData.Keywords;

            var shader = (Shader)_shader.objectReferenceValue;

            if (shader)
            {
                var shaderPropertyNames = new List<string>();
                var shaderPropertyPropertyNames = new List<string>();
                var propertyCount = ShaderUtil.GetPropertyCount(shader);
                _newProperties = new List<KeyValuePair<int, ShaderProperty>>();
                var index = 0;

                for (int i = 0; i < propertyCount; i++)
                {
                    var attrs = shader.GetPropertyAttributes(i);

                    if (attrs.Where(attr => attr == "MateriaFloat").Count() > 0 || attrs.Where(attr => attr == "MateriaColor").Count() > 0)
                    {
                        var propName = ShaderUtil.GetPropertyDescription(shader, i);
                        shaderPropertyNames.Add(propName);

                        var propPropertyName = ShaderUtil.GetPropertyName(shader, i);
                        shaderPropertyPropertyNames.Add(propPropertyName);

                        if (shaderDataProperties.Where(n => n.PropertyName == propPropertyName).Count() == 0)
                        {
                            ShaderProperty prop = null;

                            if (attrs.Contains("MateriaColor"))
                            {
                                prop = new ColorShaderProperty(propName, propPropertyName);
                            }
                            else if (attrs.Contains("MateriaFloat"))
                            {
                                prop = new FloatShaderProperty(propName, propPropertyName);
                            }

                            if (prop != null && !shaderDataProperties.Contains(prop))
                            {
                                shaderDataProperties.Insert(index, prop);
                                _newProperties.Add(new KeyValuePair<int, ShaderProperty>(index, prop));
                            }
                        }
                        index++;
                    }

                    if (attrs.Where(attr => attr == "MateriaKeyword").Count() > 0)
                    {
                        var prop = ShaderUtil.GetPropertyName(shader, i);

                        if (keywords.Where(n => n == prop).Count() == 0)
                        {
                            if (attrs.Contains("MateriaKeyword"))
                            {
                                keywords.Add(prop);
                            }
                        }
                    }
                }

                _propertiesToRemove.Clear();

                for (var i = 0; i < _shaderData.Properties.Count; i++)
                {
                    if (!shaderPropertyPropertyNames.Contains(_shaderData.Properties[i].PropertyName))
                    {
                        _propertiesToRemove.Add(_shaderData.Properties[i].PropertyName);
                        _shaderData.Properties.RemoveAt(i);
                    }
                }

                UpdateMaterias();
            }
        }

        private void UpdateMaterias()
        {
            var materias = AssetUtils.FindAssets<Materia>();

            foreach (var materia in materias)
            {
                if (materia.MaterialData?.ShaderData == _shaderData)
                {
                    RemoveOldPropertiesFromMateria(materia);

                    AddNewPropertiesToMateria(materia);

                    UpdateMateriaFloatPropertyDescriptiveNames(materia);
                }
            }
        }

        private void RemoveOldPropertiesFromMateria(Materia materia)
        {
            for (var j = 0; j < materia.Properties.Count; j++)
                for (int k = 0; k < _propertiesToRemove.Count; k++)
                    if (materia.Properties[j].PropertyName == _propertiesToRemove[k])
                        materia.Properties.RemoveAt(j);
        }

        private void AddNewPropertiesToMateria(Materia materia)
        {
            foreach (var kvp in _newProperties)
                materia.Properties.Insert(kvp.Key, kvp.Value);
        }

        private void UpdateMateriaFloatPropertyDescriptiveNames(Materia materia)
        {
            var properties = materia.Properties;

            for (int i = 0; i < _shaderData.Properties.Count; i++)
            {
                var materiaProp = properties[i];
                var shaderDataProp = _shaderData.Properties[i];

                if (shaderDataProp.GetType() == typeof(FloatShaderProperty))
                {
                    var mp = (FloatShaderProperty)properties[i];
                    var sp = (FloatShaderProperty)_shaderData.Properties[i];

                    mp.RName = sp.RName;
                    mp.GName = sp.GName;
                    mp.BName = sp.BName;
                    mp.AName = sp.AName;
                }

                materiaProp.Name = shaderDataProp.Name;
            }
        }

        protected override void SetUpView()
        {
            //
        }

        protected override void RegisterCallbacks()
        {
            _updateShaderPropertiesButton.clicked += UpdateShaderProperties;
            _addPropertyToSelected.clicked += () => AddPropertyToSelected(_availableShaderPropertiesListView.selectedItems, _shaderData.AvailableShaderProperties, _shaderData.SelectedShaderProperties);
            _removePropertyFromSelected.clicked += () => AddPropertyToSelected(_selectedShaderPropertiesListView.selectedItems, _shaderData.SelectedShaderProperties, _shaderData.AvailableShaderProperties);
        }

        protected override void GetProperties()
        {
            _shader = serializedObject.FindProperty("Shader");

            _IMGUIContainer = root.Q<IMGUIContainer>("IMGUIContainer");

            _shaderObjectField = root.Q<ObjectField>("ShaderObjectField");
            _shaderObjectField.objectType = typeof(Shader);

            _availableShaderPropertiesListView = root.Q<ListView>("AvailableShaderPropertiesListView");
            _selectedShaderPropertiesListView = root.Q<ListView>("SelectedShaderPropertiesListView");

            _availableShaderProperties = serializedObject.FindProperty("AvailableShaderProperties");
            _selectedShaderProperties = serializedObject.FindProperty("SelectedShaderProperties");

            _updateShaderPropertiesButton = root.Q<Button>("UpdateShaderPropertiesButton");

            _addPropertyToSelected = root.Q<Button>("AddPropertyToSelected");
            _removePropertyFromSelected = root.Q<Button>("RemovePropertyFromSelected");
        }

        protected override void BindProperties()
        {
            _shaderObjectField.BindProperty(_shader);
        }
    }
}