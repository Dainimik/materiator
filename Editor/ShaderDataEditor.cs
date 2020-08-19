using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

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

        private void OnEnable()
        {
            _shaderData = (ShaderData)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<ShaderData>();

            IMGUIContainer defaultInspector = new IMGUIContainer(() => IMGUI());
            _IMGUIContainer.Add(defaultInspector);

            return root;
        }

        private void IMGUI()
        {
            base.DrawDefaultInspector();
        }

        private void UpdateShaderProperties()
        {
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
        }

        protected override void GetProperties()
        {
            _shader = serializedObject.FindProperty("Shader");

            _IMGUIContainer = root.Q<IMGUIContainer>("IMGUIContainer");

            _shaderObjectField = root.Q<ObjectField>("ShaderObjectField");
            _shaderObjectField.objectType = typeof(Shader);

            _updateShaderPropertiesButton = root.Q<Button>("UpdateShaderPropertiesButton");
        }

        protected override void BindProperties()
        {
            _shaderObjectField.BindProperty(_shader);
        }
    }
}