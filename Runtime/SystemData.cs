#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
#endif
using UnityEngine;

namespace Materiator
{
    public enum EditMode
    {
        Native = 0,
        Atlas = 1
    }

    public class SystemData
    {
        public static MateriatorSettings Settings { get { return LoadSettings(); } }
        public static MateriaTagCollection MateriaTags = Settings.MateriaTags;

        [SerializeField] private static MateriatorSettings _settings { get; set; }

        public static MateriatorSettings LoadSettings()
        {
            if (_settings == null)
            {
                _settings = Resources.Load<MateriatorSettings>("MateriatorSettings");

#if UNITY_EDITOR
                if (_settings == null) CreateDefaultSettingsData();
#endif
            }

            return _settings;
        }

#if UNITY_EDITOR
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
            _settings.HighlightColor = new Color(1f, 1f, 0f, 1f);

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

            _settings.DefaultMateria = CreateDefaultMateria("DefaultMateria");
            _settings.DefaultMaterialData = CreateDefaultMaterialData("DefaultMaterialData");
            _settings.MateriaTags = CreateDefaultTagCollection("MateriaTags");

            AssetDatabase.SaveAssets();
        }

        private static Materia CreateDefaultMateria(string name)
        {
            var materia = ScriptableObject.CreateInstance<Materia>();
            materia.name = name;

            materia.ShaderData = _settings.DefaultShaderData;
            materia.AddProperties(_settings.DefaultShaderData.Properties);

            AssetDatabase.AddObjectToAsset(materia, _settings);

            return materia;
        }

        private static string CreateDefaultShaderDataAssets()
        {
            var defaultFloatValues = new Vector4(0f, 0f, 0f, 0f);
            var valueNames = new string[] { "Metallic", "", "", "Smoothness" };

            CreateDefaultShaderData(
                Shader.Find("Universal Render Pipeline/Lit"),
                new List<ShaderProperty>
                {
                    new ColorShaderProperty("Base Color", "_BaseMap", Color.white),
                    new FloatShaderProperty("Metallic/Smoothness", "_MetallicGlossMap", defaultFloatValues, valueNames), // Combine into one
                    new FloatShaderProperty("Glossiness", "_SpecGlossMap", defaultFloatValues, valueNames), // Combine into one
                    new ColorShaderProperty("Emission Color", "_EmissionMap")
                },
                new List<string>
                {
                    "_METALLICSPECGLOSSMAP",
                    "_EMISSION"
                });

            CreateDefaultShaderData(
                Shader.Find("Universal Render Pipeline/Simple Lit"),
                new List<ShaderProperty>
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
                new List<ShaderProperty>
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
                new List<ShaderProperty>
                {
                    new ColorShaderProperty("Base Color", "_MainTex", Color.white),
                    new FloatShaderProperty("Specular/Glossiness", "_SpecGlossMap", defaultFloatValues, valueNames),
                    new ColorShaderProperty("Emission Color", "_EmissionMap")
                },
                new List<string>
                {
                    "_METALLICGLOSSMAP",
                    "_EMISSION"
                });

            AssetDatabase.SaveAssets();

            return AssetDatabase.GetAssetPath(_settings);
        }

        private static void CreateDefaultShaderData(Shader shader, List<ShaderProperty> properties, List<string> keywords)
        {
            var shaderData = ScriptableObject.CreateInstance<ShaderData>();
            shaderData.name = shader.name;
            shaderData.name.Replace(' ', '_');
            shaderData.name.Replace('/', '_');

            shaderData.Shader = shader;

            foreach (var prop in properties)
                shaderData.Properties.Add(prop);

            foreach (var kw in keywords)
                shaderData.Keywords.Add(kw);

            shaderData.IsEditable = false;

            AssetDatabase.AddObjectToAsset(shaderData, _settings);
        }

        private static MaterialData CreateDefaultMaterialData(string name)
        {
            var material = Utils.CreateMaterial(_settings.DefaultShaderData.Shader);
            material.name = "DefaultMaterial";

            var materialData = ScriptableObject.CreateInstance<MaterialData>();
            materialData.Init(_settings.DefaultShaderData, material);
            materialData.name = name;

            AssetDatabase.AddObjectToAsset(materialData, _settings);
            AssetDatabase.AddObjectToAsset(material, _settings);

            return materialData;
        }

        private static MateriaTagCollection CreateDefaultTagCollection(string name)
        {
            var collection = ScriptableObject.CreateInstance<MateriaTagCollection>();
            collection.name = name;

            collection.MateriaTags.Add(Settings.DefaultTag);

            AssetDatabase.AddObjectToAsset(collection, _settings);

            return collection;
        }
#endif
    }
}