using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

namespace Materiator
{
    [CustomEditor(typeof(ShaderData))]
    [CanEditMultipleObjects]
    public class ShaderDataEditor : MateriatorEditor
    {
        private ShaderData _shaderData;

        private IMGUIContainer _IMGUIContainer;

        private SerializedProperty _shader;

        private ObjectField _shaderObjectField;
        private Button _updateShaderProperties;

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
            var shader = (Shader)_shader.objectReferenceValue;
            var shaderDataProperties = _shaderData.Properties;
            var keywords = _shaderData.Keywords;

            if (shader)
            {
                var shaderPropertyNames = new List<string>();
                var propertyCount = ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < propertyCount; i++)
                {
                    var attrs = shader.GetPropertyAttributes(i);
                    
                    if (attrs.Where(attr => attr == "MateriaFloat").Count() > 0 || attrs.Where(attr => attr == "MateriaColor").Count() > 0)
                    {
                        var propName = ShaderUtil.GetPropertyName(shader, i);
                        shaderPropertyNames.Add(propName);

                        if (shaderDataProperties.Where(n => n.Name == propName).Count() == 0)
                        {
                            ShaderProperty prop = null;

                            if (attrs.Contains("MateriaColor"))
                            {
                                prop = new ColorShaderProperty(propName);
                            }
                            else if (attrs.Contains("MateriaFloat"))
                            {
                                prop = new FloatShaderProperty(propName);
                            }

                            if (prop != null && !shaderDataProperties.Contains(prop))
                            {
                                shaderDataProperties.Add(prop);
                            }
                        }
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

                var materias = AssetUtils.FindAssets<Materia>();
                var removedName = "";

                for (var i = 0; i < _shaderData.Properties.Count; i++)
                {
                    if (!shaderPropertyNames.Contains(_shaderData.Properties[i].Name))
                    {
                        removedName = _shaderData.Properties[i].Name;
                        _shaderData.Properties.RemoveAt(i);

                        RemovePropertyFromMaterias(materias, removedName);
                    }
                }

                UpdateMaterias(materias);
            }
        }

        private void UpdateMaterias(List<Materia> materias)
        {
            var shaderDataProperties = _shaderData.Properties;

            foreach (var materia in materias)
            {
                if (materia.ShaderData == _shaderData)
                {
                    var properties = materia.Properties;

                    for (int i = 0; i < shaderDataProperties.Count; i++)
                    {
                    }
                }
            }
        }

        private void RemovePropertyFromMaterias(List<Materia> materias, string removedName)
        {
            foreach (var materia in materias)
            {
                if (materia.ShaderData == _shaderData)
                {
                    for (var j = 0; j < materia.Properties.Count; j++)
                    {
                        if (materia.Properties[j].Name == removedName)
                        {
                            materia.Properties.RemoveAt(j);
                        }
                    }
                }
            }
        }

        protected override void GetProperties()
        {
            _shader = serializedObject.FindProperty("Shader");

            _IMGUIContainer = root.Q<IMGUIContainer>("IMGUIContainer");

            _shaderObjectField = root.Q<ObjectField>("ShaderObjectField");
            _shaderObjectField.objectType = typeof(Shader);

            _updateShaderProperties = root.Q<Button>("UpdateShaderProperties");
        }

        protected override void BindProperties()
        {
            _shaderObjectField.BindProperty(_shader);
        }

        protected override void RegisterButtons()
        {
            _updateShaderProperties.clicked += UpdateShaderProperties;
        }
    }
}