using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class SystemData
    {
        public static MateriatorSettings Settings { get { return LoadSettings(); } }

        [SerializeField] private static MateriatorSettings _settings { get; set; }

        public static MateriatorSettings LoadSettings()
        {
            if (_settings == null)
            {
                _settings = Resources.Load<MateriatorSettings>("MateriatorSettings");

                if (_settings == null) CreateDefaultSettingsData();
            }

            return _settings;
        }

        private static void CreateDefaultSettingsData()
        {
            if (!Directory.Exists("Assets/Materiator"))
                AssetDatabase.CreateFolder("Assets", "Materiator");

            if (!Directory.Exists("Assets/Materiator/Resources"))
                AssetDatabase.CreateFolder("Assets/Materiator", "Resources");

            var path = "Assets/Materiator/Resources";

            _settings = ScriptableObject.CreateInstance<MateriatorSettings>();
            AssetDatabase.CreateAsset(_settings, path + "/MateriatorSettings.asset");
            EditorUtility.SetDirty(_settings);
            AssetDatabase.ImportAsset(path + "/MateriatorSettings.asset");
            _settings = AssetDatabase.LoadAssetAtPath<MateriatorSettings>(path + "/MateriatorSettings.asset");
            _settings.PackAssets = true;

            var shaderDataPath = CreateDefaultShaderDataAssets();
            var shaderDatas = AssetDatabase.LoadAllAssetsAtPath(shaderDataPath).Where(asset => asset.GetType() == typeof(ShaderData)).Cast<ShaderData>().ToArray();

            var renderPipelineType = RenderPipelineUtils.GetActivePipelineType();
            if (renderPipelineType == RenderPipelineUtils.PipelineType.Universal)
            {
                _settings.DefaultShaderData = shaderDatas.Where(sd => sd.Shader == Shader.Find("Universal Render Pipeline/Lit")).FirstOrDefault();
            }
            else if (renderPipelineType == RenderPipelineUtils.PipelineType.BuiltIn)
            {
                _settings.DefaultShaderData = shaderDatas.Where(sd => sd.Shader == Shader.Find("Standard")).FirstOrDefault();
            }

            _settings.DefaultMaterialData = CreateDefaultMaterialData("DefaultMaterialData");
            _settings.DefaultMateria = CreateDefaultMateria("DefaultMateria");

            AssetDatabase.SaveAssets();
        }

        private static Materia CreateDefaultMateria(string name)
        {
            var materia = ScriptableObject.CreateInstance<Materia>();
            materia.name = name;

            materia.MaterialData = _settings.DefaultMaterialData;
            materia.AddProperties(_settings.DefaultShaderData.MateriatorShaderProperties);

            AssetDatabase.AddObjectToAsset(materia, _settings);

            return materia;
        }

        private static string CreateDefaultShaderDataAssets()
        {
            var defaultFloatValues = new Vector4(0f, 0f, 0f, 0f);
            var valueNames = new string[] { "Metallic", "", "", "Smoothness" };

            CreateDefaultShaderData(
                Shader.Find("Universal Render Pipeline/Lit"),
                new List<MateriatorShaderProperty>
                {
                    new MateriatorShaderProperty("Base Color", "_BaseMap", ShaderPropertyType.Color, new List<MateriatorShaderPropertyValue>()
                    {
                        new MateriatorShaderPropertyValue("R", "_BaseMap", MateriatorShaderPropertyValueChannel.R, defaultFloatValues.x),
                        new MateriatorShaderPropertyValue("G", "_BaseMap", MateriatorShaderPropertyValueChannel.G, defaultFloatValues.y),
                        new MateriatorShaderPropertyValue("B", "_BaseMap", MateriatorShaderPropertyValueChannel.B, defaultFloatValues.z),
                        new MateriatorShaderPropertyValue("A", "_BaseMap", MateriatorShaderPropertyValueChannel.A, defaultFloatValues.w),
                    }),
                    new MateriatorShaderProperty("Metallic/Smoothness", "_MetallicGlossMap", ShaderPropertyType.Float, new List<MateriatorShaderPropertyValue>()
                    {
                        new MateriatorShaderPropertyValue("Metallic", "_MetallicGlossMap", MateriatorShaderPropertyValueChannel.R, defaultFloatValues.x),
                        new MateriatorShaderPropertyValue("Smoothness", "_MetallicGlossMap", MateriatorShaderPropertyValueChannel.A, defaultFloatValues.w)
                    }), // combine into one
                    new MateriatorShaderProperty("Glossiness", "_SpecGlossMap", ShaderPropertyType.Float, new List<MateriatorShaderPropertyValue>()
                    {
                        new MateriatorShaderPropertyValue("X", "_SpecGlossMap", MateriatorShaderPropertyValueChannel.R, defaultFloatValues.x) // this probably wont work needs to be tested
                    }), // combine into one
                    new MateriatorShaderProperty("Emission Color", "_EmissionMap", ShaderPropertyType.Color, new List<MateriatorShaderPropertyValue>()
                    {
                        new MateriatorShaderPropertyValue("R", "_EmissionMap", MateriatorShaderPropertyValueChannel.R, defaultFloatValues.x),
                        new MateriatorShaderPropertyValue("G", "_EmissionMap", MateriatorShaderPropertyValueChannel.G, defaultFloatValues.y),
                        new MateriatorShaderPropertyValue("B", "_EmissionMap", MateriatorShaderPropertyValueChannel.B, defaultFloatValues.z),
                        new MateriatorShaderPropertyValue("A", "_EmissionMap", MateriatorShaderPropertyValueChannel.A, defaultFloatValues.w),
                    })
                },
                new List<string>
                {
                    "_METALLICSPECGLOSSMAP",
                    "_EMISSION"
                });

            /*CreateDefaultShaderData(
                Shader.Find("Universal Render Pipeline/Simple Lit"),
                new List<MateriatorShaderProperty>
                {
                    new ColorShaderProperty("Base Color", "_BaseMap", Color.white),
                    new FloatShaderProperty("Specular/Glossiness", "_SpecGlossMap", defaultFloatValues, valueNames),
                    new ColorShaderProperty("Emission Color", "_EmissionMap")
                },
                new List<string>
                {
                    "_METALLICSPECGLOSSMAP",
                    "_EMISSION"
                });

            CreateDefaultShaderData(
                Shader.Find("Standard"),
                new List<MateriatorShaderProperty>
                {
                    new ColorShaderProperty("Base Color", "_MainTex", Color.white),
                    new FloatShaderProperty("Metallic/Smoothness/Glossiness", "_MetallicGlossMap", defaultFloatValues, valueNames),
                    new ColorShaderProperty("Emission Color", "_EmissionMap")
                },
                new List<string>
                {
                    "_METALLICGLOSSMAP",
                    "_EMISSION"
                });

            CreateDefaultShaderData(
                Shader.Find("Standard (Specular setup)"),
                new List<MateriatorShaderProperty>
                {
                    new ColorShaderProperty("Base Color", "_MainTex", Color.white),
                    new FloatShaderProperty("Specular/Glossiness", "_SpecGlossMap", defaultFloatValues, valueNames),
                    new ColorShaderProperty("Emission Color", "_EmissionMap")
                },
                new List<string>
                {
                    "_METALLICGLOSSMAP",
                    "_EMISSION"
                });*/

            AssetDatabase.SaveAssets();

            return AssetDatabase.GetAssetPath(_settings);
        }

        private static void CreateDefaultShaderData(Shader shader, List<MateriatorShaderProperty> properties, List<string> keywords)
        {
            var shaderData = ScriptableObject.CreateInstance<ShaderData>();
            shaderData.name = shader.name;
            shaderData.name.Replace(' ', '_');
            shaderData.name.Replace('/', '_');

            shaderData.Shader = shader;

            foreach (var prop in properties)
                shaderData.MateriatorShaderProperties.Add(prop);

            foreach (var kw in keywords)
                shaderData.Keywords.Add(kw);

            shaderData.IsEditable = false;

            AssetDatabase.AddObjectToAsset(shaderData, _settings);
        }

        private static MaterialData CreateDefaultMaterialData(string name)
        {
            var material = MaterialUtils.CreateMaterial(_settings.DefaultShaderData.Shader);
            material.name = "DefaultMaterial";

            var materialData = ScriptableObject.CreateInstance<MaterialData>();
            materialData.Init(_settings.DefaultShaderData, material);
            materialData.name = name;

            AssetDatabase.AddObjectToAsset(materialData, _settings);
            AssetDatabase.AddObjectToAsset(material, _settings);

            return materialData;
        }
    }
}