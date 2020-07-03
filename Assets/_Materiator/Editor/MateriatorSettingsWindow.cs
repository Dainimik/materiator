using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class MateriatorSettingsWindow : EditorWindow
    {
        private Editor _editor = null;
        private MateriatorSettings _settings;

        [MenuItem("Tools/Materiator/Settings")]
        public static void OpenWindow()
        {
            GetWindow<MateriatorSettingsWindow>("Materiator Settings");
        }

        private void OnEnable()
        {
            _settings = Resources.Load<MateriatorSettings>("MateriatorSettings");
            if (_settings == null) CreateDefaultSettingsData();
            if (!_editor) Editor.CreateCachedEditor(_settings, null, ref _editor);
        }

        private void OnGUI()
        {
            if (_editor) _editor.OnInspectorGUI();
        }

        private void CreateDefaultSettingsData()
        {
            var editorScriptPath = AssetUtils.GetEditorScriptDirectory(this);
            AssetUtils.CheckDirAndCreate(editorScriptPath, "Resources");
            var path = editorScriptPath + "/Resources";
            _settings = AssetUtils.CreateScriptableObjectAsset<MateriatorSettings>(path, "MateriatorSettings");
            _settings.PackAssets = true;
            _settings.DefaultMateria = AssetUtils.CreateScriptableObjectAsset<Materia>(path, "DefaultMateria");
            _settings.DefaultMateria.BaseColor = Color.gray;
            _settings.HighlightColor = new Color(1f, 1f, 0f, 1f);
            //_settings.GlobalIlluminationFlag = MaterialGlobalIlluminationFlags.None;
            var shaderDataPath = CreateDefaultShaderDataAssets();

            var renderPipelineType = RenderPipelineUtils.GetActivePipelineType();
            if (renderPipelineType == RenderPipelineUtils.PipelineType.Universal)
            {
                _settings.DefaultShaderData = AssetDatabase.LoadAssetAtPath<ShaderData>(shaderDataPath + "Lit_UniversalRenderPipeline.asset");
            }
            else if (renderPipelineType == RenderPipelineUtils.PipelineType.BuiltIn)
            {
                _settings.DefaultShaderData = AssetDatabase.LoadAssetAtPath<ShaderData>(shaderDataPath + "Standard_BuiltInRenderPipeline.asset");
            }

            CreateMaterialData("DefaultMaterial");

            return;
        }

        private string CreateDefaultShaderDataAssets()
        {
            string path = AssetUtils.GetEditorScriptDirectory(this);
            AssetUtils.CheckDirAndCreate(path + "/Resources", "ShaderData");
            path += "/Resources/ShaderData/";
            CreateShaderData("Universal Render Pipeline/Lit", "_BaseColor", "_BaseMap", "_MetallicGlossMap", "_SpecGlossMap", "_EmissionColor", "_EmissionMap", "_METALLICSPECGLOSSMAP", "_EMISSION", path, "Lit_UniversalRenderPipeline");
            CreateShaderData("Universal Render Pipeline/Simple Lit", "_BaseColor", "_BaseMap", null, "_SpecGlossMap", "_EmissionColor", "_EmissionMap", "_METALLICSPECGLOSSMAP", "_EMISSION", path, "SimpleLit_UniversalRenderPipeline");
            CreateShaderData("Standard", "_Color", "_MainTex", "_MetallicGlossMap", null, "_EmissionColor", "_EmissionMap", "_METALLICGLOSSMAP", "_EMISSION", path, "Standard_BuiltInRenderPipeline");
            CreateShaderData("Standard (Specular setup)", "_Color", "_MainTex", null, "_SpecGlossMap", "_EmissionColor", "_EmissionMap", "_METALLICGLOSSMAP", "_EMISSION", path, "StandardSpecular_BuiltInRenderPipeline");
            return path;
        }

        private void CreateShaderData(string shaderName, string mainColorPropertyName, string mainTexturePropertyName, string metallicSmoothnessTexturePropertyName, string specularGlossTExturePropertyName, string emissionColorPropertyName, string emissionTexturePropertyName, string metallicSmoothnessKeywordName, string emissionKeywordName, string absolutePath, string shaderDataName)
        {
            var shaderData = AssetUtils.CreateScriptableObjectAsset<ShaderData>(AssetUtils.AbsoluteToRelativePath(absolutePath), shaderDataName);
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
        }

        private void CreateMaterialData(string name)
        {
            string path = AssetUtils.GetEditorScriptDirectory(this);
            AssetUtils.CheckDirAndCreate(path + "/Resources", "MaterialData");
            path += "/Resources/MaterialData/";

            var materialData = AssetUtils.CreateScriptableObjectAsset<MaterialData>(AssetUtils.AbsoluteToRelativePath(path), name);
            materialData.ShaderData = Utils.Settings.DefaultShaderData;

            var material = Utils.CreateMaterial(materialData.ShaderData.Shader, name);
            materialData.Material = material;

            AssetDatabase.AddObjectToAsset(material, materialData);
        }
    }
}