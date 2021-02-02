using UnityEngine;
using UnityEditor;
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

        private Button _updateMateriasButton;

        private List<KeyValuePair<int, MateriatorShaderProperty>> _newProperties;

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

        private int GetRequiredTextureCount(List<ShaderProperty> shaderProperties)
        {
            var count = 0f;

            foreach (var prop in shaderProperties)
                count += prop.Value.Count;  

            return Mathf.CeilToInt(count / 4);
        }

        private void UpdateShaderProperties()
        {
            var shaderDataProperties = _shaderData.MateriatorShaderProperties;
            var keywords = _shaderData.Keywords;

            var shader = (Shader)_shader.objectReferenceValue;

            if (shader)
            {
                var shaderPropertyNames = new List<string>();
                var shaderPropertyPropertyNames = new List<string>();
                var propertyCount = ShaderUtil.GetPropertyCount(shader);
                _newProperties = new List<KeyValuePair<int, MateriatorShaderProperty>>();
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
                            MateriatorShaderProperty prop = null;

                            if (attrs.Contains("MateriaColor"))
                            {
                                //prop = new ColorShaderProperty(propName, propPropertyName);
                            }
                            else if (attrs.Contains("MateriaFloat"))
                            {
                                //prop = new FloatShaderProperty(propName, propPropertyName);
                            }

                            if (prop != null && !shaderDataProperties.Contains(prop))
                            {
                                shaderDataProperties.Insert(index, prop);
                                _newProperties.Add(new KeyValuePair<int, MateriatorShaderProperty>(index, prop));
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

                for (var i = 0; i < _shaderData.MateriatorShaderProperties.Count; i++)
                {
                    if (!shaderPropertyPropertyNames.Contains(_shaderData.MateriatorShaderProperties[i].PropertyName))
                    {
                        _propertiesToRemove.Add(_shaderData.MateriatorShaderProperties[i].PropertyName);
                        _shaderData.MateriatorShaderProperties.RemoveAt(i);
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
                if (materia.MaterialData.ShaderData != _shaderData)
                    continue;

                RemoveOldPropertiesFromMateria(materia);
                AddNewPropertiesToMateria(materia);

                EditorUtility.SetDirty(materia);
                //UpdateMateriaFloatPropertyDescriptiveNames(materia);
            }

            AssetDatabase.SaveAssets();
        }

        private void RemoveOldPropertiesFromMateria(Materia materia)
        {
            for (int i = 0; i < materia.Properties.Count; i++)
            {
                var prop = materia.Properties[i];

                if (_shaderData.MateriatorShaderProperties.Where(p => p.PropertyName == prop.PropertyName).Count() == 0)
                {
                    materia.Properties.Remove(prop);
                }

                for (int j = 0; j < prop.Values.Count; j++)
                {
                    var value = prop.Values[j];

                    if (_shaderData.MateriatorShaderProperties.SelectMany(p => p.Values).Where(v => v.PropertyName == value.PropertyName).Count() == 0)
                    {
                        prop.Values.Remove(value);
                    }
                }
            }
            //for (var j = 0; j < materia.Properties.Count; j++)
            //    for (int k = 0; k < _propertiesToRemove.Count; k++)
            //       if (materia.Properties[j].PropertyName == _propertiesToRemove[k])
            //            materia.Properties.RemoveAt(j);
        }

        private void AddNewPropertiesToMateria(Materia materia)
        {
            for (int i = 0; i < _shaderData.MateriatorShaderProperties.Count; i++)
            {
                var prop = _shaderData.MateriatorShaderProperties[i];

                if (materia.Properties.Where(p => p.PropertyName == prop.PropertyName).Count() == 0)
                {
                    materia.Properties.Insert(_shaderData.MateriatorShaderProperties.IndexOf(prop), ObjectCopier.Clone(prop));
                    //materia.Properties.Insert(_shaderData.MateriatorShaderProperties.IndexOf(prop), prop);
                }

                for (int j = 0; j < prop.Values.Count; j++)
                {
                    var value = prop.Values[j];
                    var materiaPropValues = materia.Properties.Where(p => p.PropertyName == prop.PropertyName).FirstOrDefault().Values;

                    if (materiaPropValues.Where(v => v.PropertyName == value.PropertyName).Count() == 0)
                    {
                        materiaPropValues.Insert(_shaderData.MateriatorShaderProperties[i].Values.IndexOf(value), ObjectCopier.Clone(value));
                    }
                }
            }

            //foreach (var kvp in _newProperties)
            //    materia.Properties.Insert(kvp.Key, kvp.Value);
        }

        private void UpdateMateriaFloatPropertyDescriptiveNames(Materia materia)
        {
            var properties = materia.Properties;

            for (int i = 0; i < _shaderData.MateriatorShaderProperties.Count; i++)
            {
                var materiaProp = properties[i];
                var shaderDataProp = _shaderData.MateriatorShaderProperties[i];

                /*if (shaderDataProp.GetType() == typeof(FloatShaderProperty))
                {
                    var mp = (FloatShaderProperty)properties[i];
                    var sp = (FloatShaderProperty)_shaderData.MateriatorShaderProperties[i];

                    mp.RName = sp.RName;
                    mp.GName = sp.GName;
                    mp.BName = sp.BName;
                    mp.AName = sp.AName;
                }*/

                materiaProp.Name = shaderDataProp.Name;
            }
        }

        protected override void SetUpView()
        {
            //
        }

        protected override void RegisterCallbacks()
        {
            _updateMateriasButton.clicked += UpdateMaterias;
        }

        protected override void GetProperties()
        {
            _shader = serializedObject.FindProperty("Shader");

            _IMGUIContainer = root.Q<IMGUIContainer>("IMGUIContainer");
            _updateMateriasButton = root.Q<Button>("UpdateMateriasButton");
        }

        protected override void BindProperties()
        {

        }
    }
}