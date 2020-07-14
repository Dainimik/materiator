#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
#endif
using UnityEngine;

namespace Materiator
{
    public class SystemData
    {
        public static MateriatorSettings Settings { get { return LoadSettings(); } }
        public static MateriaTags MateriaTags = Settings.MateriaTags;

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
            if (!Directory.Exists("Assets" + "/" + "Materiator"))
                AssetDatabase.CreateFolder("Assets", "Materiator");

            if (!Directory.Exists("Assets/Materiator" + "/" + "Resources"))
                AssetDatabase.CreateFolder("Assets/Materiator", "Resources");

            var path = "Assets/Materiator/Resources";

            _settings = ScriptableObject.CreateInstance<MateriatorSettings>();
            AssetDatabase.CreateAsset(_settings, path + "/MateriatorSettings.asset");
            EditorUtility.SetDirty(_settings);
            AssetDatabase.ImportAsset(path + "/MateriatorSettings.asset");
            _settings = AssetDatabase.LoadAssetAtPath<MateriatorSettings>(path + "/MateriatorSettings.asset");


            _settings.PackAssets = true;
            _settings.HighlightColor = new Color(1f, 1f, 0f, 1f);
            //_settings.GlobalIlluminationFlag = MaterialGlobalIlluminationFlags.None;

            var shaderDataPath = CreateDefaultShaderDataAssets();
            var shaderDatas = AssetDatabase.LoadAllAssetsAtPath(shaderDataPath).Where(asset => asset.GetType() == typeof(ShaderData)).Cast<ShaderData>().ToArray();

            var renderPipelineType = RenderPipelineUtils.GetActivePipelineType();
            if (renderPipelineType == RenderPipelineUtils.PipelineType.Universal)
            {
                //_settings.DefaultShaderData = AssetDatabase.LoadAssetAtPath<ShaderData>(shaderDataPath + "Lit_UniversalRenderPipeline");
                _settings.DefaultShaderData = shaderDatas.Where(sd => sd.Shader == Shader.Find("Universal Render Pipeline/Lit")).FirstOrDefault();
            }
            else if (renderPipelineType == RenderPipelineUtils.PipelineType.BuiltIn)
            {
                _settings.DefaultShaderData = AssetDatabase.LoadAssetAtPath<ShaderData>(shaderDataPath + "Standard_BuiltInRenderPipeline.asset");
            }

            _settings.DefaultMateria = CreateDefaultMateria("DefaultMateria");
            _settings.DefaultMaterialData = CreateMaterialData("DefaultMaterialData");
            _settings.MateriaTags = CreateDefaultTagsData("MateriaTags");

            AssetDatabase.SaveAssets();

            return;
        }

        private static Materia CreateDefaultMateria(string name)
        {
            var materia = ScriptableObject.CreateInstance<Materia>();
            materia.name = name;
            materia.BaseColor = Color.gray;

            AssetDatabase.AddObjectToAsset(materia, _settings);

            return materia;
        }

        private static string CreateDefaultShaderDataAssets()
        {
            CreateShaderData("Universal Render Pipeline/Lit", "_BaseColor", "_BaseMap", "_MetallicGlossMap", "_SpecGlossMap", "_EmissionColor", "_EmissionMap", "_METALLICSPECGLOSSMAP", "_EMISSION", "Lit_UniversalRenderPipeline");
            CreateShaderData("Universal Render Pipeline/Simple Lit", "_BaseColor", "_BaseMap", null, "_SpecGlossMap", "_EmissionColor", "_EmissionMap", "_METALLICSPECGLOSSMAP", "_EMISSION", "SimpleLit_UniversalRenderPipeline");
            CreateShaderData("Standard", "_Color", "_MainTex", "_MetallicGlossMap", null, "_EmissionColor", "_EmissionMap", "_METALLICGLOSSMAP", "_EMISSION", "Standard_BuiltInRenderPipeline");
            CreateShaderData("Standard (Specular setup)", "_Color", "_MainTex", null, "_SpecGlossMap", "_EmissionColor", "_EmissionMap", "_METALLICGLOSSMAP", "_EMISSION", "StandardSpecular_BuiltInRenderPipeline");

            AssetDatabase.SaveAssets();

            return AssetDatabase.GetAssetPath(_settings);
        }

        private static void CreateShaderData(string shaderName, string mainColorPropertyName, string mainTexturePropertyName, string metallicSmoothnessTexturePropertyName, string specularGlossTExturePropertyName, string emissionColorPropertyName, string emissionTexturePropertyName, string metallicSmoothnessKeywordName, string emissionKeywordName, string shaderDataName)
        {
            var shaderData = ScriptableObject.CreateInstance<ShaderData>();
            shaderData.name = shaderDataName;

            shaderData.Shader = Shader.Find(shaderName);
            shaderData.BaseColorPropertyName = mainColorPropertyName;
            shaderData.MainTexturePropertyName = mainTexturePropertyName;
            shaderData.MetallicSmoothnessTexturePropertyName = metallicSmoothnessTexturePropertyName;
            shaderData.SpecularGlossTexturePropertyName = specularGlossTExturePropertyName;
            shaderData.EmissionColorPropertyName = emissionColorPropertyName;
            shaderData.EmissionTexturePropertyName = emissionTexturePropertyName;
            shaderData.MetallicSmoothnessKeywordName = metallicSmoothnessKeywordName;
            shaderData.EmissionKeywordName = emissionKeywordName;
            shaderData.IsEditable = false;

            AssetDatabase.AddObjectToAsset(shaderData, _settings);
        }

        private static MaterialData CreateMaterialData(string name)
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

        private static MateriaTags CreateDefaultTagsData(string name)
        {
            var materiaTags = ScriptableObject.CreateInstance<MateriaTags>();
            materiaTags.name = name;

            materiaTags.MateriaTagsList.Add("-");
            materiaTags.MateriaTagsList.Add("Metal");
            materiaTags.MateriaTagsList.Add("Plastic");

            AssetDatabase.AddObjectToAsset(materiaTags, _settings);

            return materiaTags;
        }
#endif
    }
}