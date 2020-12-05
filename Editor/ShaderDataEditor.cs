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

        private ObjectField _sourceShaderObjectField;
        private Button _updateShaderPropertiesButton;

        private List<KeyValuePair<int, MateriatorShaderProperty>> _newProperties;

        private Button _convertShaderPropertiesButton;

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

        private void ConvertShaderProperties()
        {
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _shaderData.MateriatorShaderProperties = CreateMateriatorShaderProperties();


        }

        private List<MateriatorShaderProperty> CreateMateriatorShaderProperties()
        {
            /*var properties = new List<MateriatorShaderProperty>();

            var shaderPath = Path.GetFullPath(AssetDatabase.GetAssetPath(_shaderData.Shader));

            var colorShaderProperties = _shaderData.SelectedShaderProperties.Where(prop => prop.Type == ShaderPropertyType.Color).ToList();  

            foreach (var shaderProp in colorShaderProperties)
            {
                var materiatorColorShaderPropertyValues = new List<MateriatorShaderPropertyValue>()
                {
                    new MateriatorShaderPropertyValue("R", "r", MateriatorShaderPropertyValueChannel.R, float.Parse(shaderProp.Value[0])),
                    new MateriatorShaderPropertyValue("G", "g", MateriatorShaderPropertyValueChannel.G, float.Parse(shaderProp.Value[1])),
                    new MateriatorShaderPropertyValue("B", "b", MateriatorShaderPropertyValueChannel.B, float.Parse(shaderProp.Value[2])),
                    new MateriatorShaderPropertyValue("A", "a", MateriatorShaderPropertyValueChannel.A, float.Parse(shaderProp.Value[3])),
                };

                var materiatorColorShaderProperty = new MateriatorShaderProperty(shaderProp.Name, "_Materiator" + shaderProp.PropertyName, shaderProp.Type, materiatorColorShaderPropertyValues);

                properties.Add(materiatorColorShaderProperty);

                var val0 = Environment.NewLine +
                    "TEXTURE2D(" + materiatorColorShaderProperty.PropertyName + ");" +
                    Environment.NewLine +
                    "SAMPLER(sampler" + materiatorColorShaderProperty.PropertyName + ");" +
                    Environment.NewLine + Environment.NewLine;

                var val = Environment.NewLine +
                    shaderProp.PropertyName +
                    " = " + "SAMPLE_TEXTURE2D(" +
                    materiatorColorShaderProperty.PropertyName +
                    ", sampler" + materiatorColorShaderProperty.PropertyName +
                    ", materiator_uv.xy );";

                IOUtils.InsertStringBefore(val0, @"(float[2-4]|fixed[2-4]|half[2-4])\s*frag.*\)\s*:\s*(?i)SV_TARGET\s*\{", shaderPath);
                IOUtils.InsertStringAfter(val , @"(float[2-4]|fixed[2-4]|half[2-4])\s*frag.*\)\s*:\s*(?i)SV_TARGET\s*\{", shaderPath);

                var texProp = Environment.NewLine + materiatorColorShaderProperty.PropertyName + "(\"" + materiatorColorShaderProperty.Name + "\", 2D) = \"white\" {}";
                IOUtils.InsertStringAfter(texProp, @"\s*Properties.*\s*\{", shaderPath);
            }


            var vectorShaderProperties = _shaderData.SelectedShaderProperties.Where(prop => prop.Type == ShaderPropertyType.Vector).ToList();
            var texCount = GetRequiredTextureCount(vectorShaderProperties);
            //Right now, every unique vector shader property gets a dedicated channel. Need to do so that every value of a property gets a dedicated channel!
            MateriatorShaderProperty materiatorVectorShaderProperty = null;
            List<MateriatorShaderPropertyValue> materiatorShaderPropertyValues = null;

            var j = 0;
            var rgbaCounter = 0;
            foreach (var shaderProp in vectorShaderProperties)
            {
                for (int i = 0; i < shaderProp.Value.Count; i++)
                {
                    var value = shaderProp.Value[i];
                    MateriatorShaderPropertyValueChannel valueChannel = default;

                    switch (rgbaCounter % 4)
                    {
                        case 0:
                            valueChannel = MateriatorShaderPropertyValueChannel.R;
                            break;
                        case 1:
                            valueChannel = MateriatorShaderPropertyValueChannel.G;
                            break;
                        case 2:
                            valueChannel = MateriatorShaderPropertyValueChannel.B;
                            break;
                        case 3:
                            valueChannel = MateriatorShaderPropertyValueChannel.A;
                            break;
                        default:
                            break;
                    }

                    if (materiatorShaderPropertyValues == null || materiatorShaderPropertyValues.Count == 0 || materiatorShaderPropertyValues.Count == 4)
                    {
                        materiatorShaderPropertyValues = new List<MateriatorShaderPropertyValue>();
                    }

                    materiatorShaderPropertyValues.Add(new MateriatorShaderPropertyValue(shaderProp.Name, shaderProp.PropertyName, valueChannel, float.Parse(value)));

                    if (rgbaCounter % 4 == 0)
                    {
                        j++;
                        materiatorVectorShaderProperty = new MateriatorShaderProperty("Materiator Property " + j, "_MateriatorProperty" + j, shaderProp.Type, materiatorShaderPropertyValues);
                        properties.Add(materiatorVectorShaderProperty);

                        var val0 = Environment.NewLine +
                            "TEXTURE2D(" + materiatorVectorShaderProperty.PropertyName + ");" +
                            Environment.NewLine +
                            "SAMPLER(sampler" + materiatorVectorShaderProperty.PropertyName + ");" +
                            Environment.NewLine + Environment.NewLine;

                        IOUtils.InsertStringBefore(val0, @"(float[2-4]|fixed[2-4]|half[2-4])\s*frag.*\)\s*:\s*(?i)SV_TARGET\s*\{", shaderPath);

                        var texProp = Environment.NewLine + materiatorVectorShaderProperty.PropertyName + "(\"" + materiatorVectorShaderProperty.Name + "\", 2D) = \"white\" {}";
                        IOUtils.InsertStringAfter(texProp, @"\s*Properties.*\s*\{", shaderPath);
                    }

                    var rgbaValue = "";
                    switch (rgbaCounter % 4)
                    {
                        case 0:
                            rgbaValue = "r";
                            break;
                        case 1:
                            rgbaValue = "g";
                            break;
                        case 2:
                            rgbaValue = "b";
                            break;
                        case 3:
                            rgbaValue = "a";
                            break;
                        default:
                            break;
                    }

                    var val = Environment.NewLine +
                        shaderProp.PropertyName +
                        " = " + "SAMPLE_TEXTURE2D(" +
                        "_MateriatorProperty" + j +
                        ", sampler_MateriatorProperty" + j +
                        ", materiator_uv.xy )." + rgbaValue + ";";

                    IOUtils.InsertStringAfter(val, @"(float[2-4]|fixed[2-4]|half[2-4])\s*frag.*\)\s*:\s*(?i)SV_TARGET\s*\{", shaderPath);

                    rgbaCounter++;
                }
            }



            var vertexFunctionName = "vert";
            var fragmentFunctionName = "frag";
            var texcoord = "TEXCOORD0";
            var vertexUvVal = ", out float4 materiator_uv : " + texcoord + "";
            var fragmentUvVal = ", in float4 materiator_uv : " + texcoord + "";

            var vertexOriginals = ShaderParser.ParseFile(shaderPath, vertexFunctionName + @"\s*\(.*\)");
            foreach (var original in vertexOriginals)
            {
                var modified = original.Replace(")", vertexUvVal + ")");
                IOUtils.ReplaceInFile(shaderPath, original, modified);
            }

            var texcoordAssign = "materiator_uv = v.texcoord";
            IOUtils.InsertStringAfter(texcoordAssign, vertexFunctionName + @"\s*\(.*\)\s*\{", shaderPath);

            var fragmentOriginals = ShaderParser.ParseFile(shaderPath, fragmentFunctionName + @"\s*\(.*\)");
            foreach (var original in fragmentOriginals)
            {
                var modified = original.Replace(")", fragmentUvVal + ")");
                IOUtils.ReplaceInFile(shaderPath, original, modified);
            }*/
            var properties = new List<MateriatorShaderProperty>();
            return properties;
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
            
        }

        protected override void GetProperties()
        {
            _shader = serializedObject.FindProperty("Shader");

            _IMGUIContainer = root.Q<IMGUIContainer>("IMGUIContainer");
        }

        protected override void BindProperties()
        {

        }
    }
}