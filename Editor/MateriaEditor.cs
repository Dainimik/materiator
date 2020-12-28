using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditorInternal;
using System;

namespace Materiator
{
    [CustomEditor(typeof(Materia))]
    [CanEditMultipleObjects]
    public class MateriaEditor : MateriatorEditor
    {
        private Materia _materia;

        private VisualElement _IMGUIContainer;

        private SerializedProperty _materialData;
        private SerializedProperty _description;

        private ReorderableList _materiaPropertyList;

        private ObjectField _materialDataObjectField;
        private TextField _descriptionTextField;

        private Button _createMateriaButton;
        private Button _restoreDefaultsButton;
        private Button _applyChangesButton;

        private PreviewRenderUtility _previewRenderUtility;
        private Mesh _previewMesh;
        private Material _previewMaterial;
        private Textures _previewTextures;
        private Texture _previewTexture;
        private Vector2 _drag;

        private void OnEnable()
        {
            _materia = (Materia)target;

            UpdatePreview();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _previewRenderUtility?.Cleanup();
        }

        private void OnDestroy()
        {
            _previewRenderUtility?.Cleanup();
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<Materia>();

            _materiaPropertyList = new ReorderableList(serializedObject, serializedObject.FindProperty("Properties"), false, true, false, false);
            SetUpList();

            IMGUIContainer defaultInspector = new IMGUIContainer(() => IMGUI());
            _IMGUIContainer.Add(defaultInspector);

            return root;
        }

        private void IMGUI()
        {
            serializedObject.Update();
            
            _materiaPropertyList.DoLayoutList();           

            serializedObject.ApplyModifiedProperties();
        }

        private void OnValueChanged()
        {
            UpdatePreview();

            if (_materia.Properties.Count > 0)
                _materia.PreviewIcon = GenerateThumbnail();

            SetUpView();
        }

        private void CreateMateria()
        {
            if (!_materia.MaterialData.ShaderData) return;

            var shaderDataProperties = _materia.MaterialData.ShaderData.MateriatorShaderProperties;

            _materia.Properties.Clear();

            foreach (var prop in shaderDataProperties)
                _materia.Properties.Add(ObjectCopier.Clone(prop));

            OnValueChanged();
            SetUpView();
        }

        private void RestoreDefaults()
        {
            if (EditorUtility.DisplayDialog("Restore Defaults", "All current settings will be lost. Are you sure?", "Yes", "No"))
            {
                CreateMateria();
                OnValueChanged();
            }
        }

        private void UpdateMateriaAtlases()
        {
            var atlases = AssetUtils.FindAssets<MateriaAtlas>();

            foreach (var atlas in atlases)
            {
                AtlasFactory.GenerateAtlas(atlas);
            }
        }

        private void SetUpList()
        {
            _materiaPropertyList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(new Rect(rect.x + 25f, rect.y, 200f, 20f), new GUIContent("Shader Properties", "Shader Properties"), EditorStyles.boldLabel);
            };

            _materiaPropertyList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var property = _materia.Properties[index];

                EditorGUI.BeginChangeCheck();
                CreateShaderPropertyGUI(rect, property);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();

                    OnValueChanged();
                }
            };

            _materiaPropertyList.elementHeightCallback = (int index) =>
            {
                var element = _materiaPropertyList.serializedProperty.GetArrayElementAtIndex(index);
                var property = _materia.Properties[index];
                var propertyHeight = EditorGUI.GetPropertyHeight(element, true);
                var heightCorrection = EditorGUIUtility.singleLineHeight;

                if (property.Type == ShaderPropertyType.Vector3 || property.Type == ShaderPropertyType.Vector2 || property.Type == ShaderPropertyType.Vector4 || property.Type == ShaderPropertyType.Float)
                {
                    heightCorrection += EditorGUIUtility.singleLineHeight;
                }

                return propertyHeight + heightCorrection;
            };
        }

        private void CreateShaderPropertyGUI(Rect rect, MateriatorShaderProperty element)
        {
            var prop = element;

            switch (prop.Type)
            {
                case ShaderPropertyType.Color:
                    var colorValue = EditorGUI.ColorField(rect, new GUIContent(prop.Name), new Color(prop.Values[0].Value, prop.Values[1].Value, prop.Values[2].Value, prop.Values[3].Value), true, true, true);
                    prop.Values[0].Value = colorValue.r;
                    prop.Values[1].Value = colorValue.g;
                    prop.Values[2].Value = colorValue.b;
                    prop.Values[3].Value = colorValue.a;
                    break;
                case ShaderPropertyType.Float:
                    for (int i = 0; i < prop.Values.Count; i++)
                    {
                        Rect r = new Rect(rect.x, rect.y + (i * EditorGUIUtility.singleLineHeight), rect.width, EditorGUIUtility.singleLineHeight);
                        var floatValue = EditorGUI.FloatField(r, new GUIContent(prop.Values[i].Name), prop.Values[i].Value);
                        prop.Values[i].Value = floatValue;
                    }
                    break;
                case ShaderPropertyType.Vector2:
                    if (prop.Values.Count == 2)
                    {
                        var vector2Value = EditorGUI.Vector2Field(rect, new GUIContent(prop.Name), new Vector2(prop.Values[0].Value, prop.Values[1].Value));
                        prop.Values[0].Value = vector2Value.x;
                        prop.Values[1].Value = vector2Value.y;
                    }
                    break;
                case ShaderPropertyType.Vector3:
                    if (prop.Values.Count == 3)
                    {
                        var vector3Value = EditorGUI.Vector3Field(rect, new GUIContent(prop.Name), new Vector3(prop.Values[0].Value, prop.Values[1].Value, prop.Values[2].Value));
                        prop.Values[0].Value = vector3Value.x;
                        prop.Values[1].Value = vector3Value.y;
                        prop.Values[2].Value = vector3Value.z;
                    }
                    break;
                case ShaderPropertyType.Vector4:
                    if (prop.Values.Count == 4)
                    {
                        var vector4Value = EditorGUI.Vector4Field(rect, new GUIContent(prop.Name), new Vector4(prop.Values[0].Value, prop.Values[1].Value, prop.Values[2].Value, prop.Values[3].Value));
                        prop.Values[0].Value = vector4Value.x;
                        prop.Values[1].Value = vector4Value.y;
                        prop.Values[2].Value = vector4Value.z;
                        prop.Values[3].Value = vector4Value.w;
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnShaderDataChanged(ChangeEvent<UnityEngine.Object> e)
        {
            _materialData.objectReferenceValue = e.newValue;
            serializedObject.ApplyModifiedProperties();

            SetUpView();
        }

        #region Preview
        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            if (_materia == null || _materia.PreviewIcon == null)
                return null;

            Texture2D staticPreviewTex = new Texture2D(width, height);
            EditorUtility.CopySerialized(_materia.PreviewIcon, staticPreviewTex);

            return staticPreviewTex;
        }

        private void SetUpPreview()
        {
            if (_materia.MaterialData)
            {
                _previewMesh = AssetUtils.LoadAssetFromUniqueAssetPath<Mesh>("Library/unity default resources::Sphere");
                _previewMaterial = Instantiate(_materia.MaterialData.Material);

                foreach (var kw in _materia.MaterialData.ShaderData.Keywords)
                {
                    if (!_previewMaterial.IsKeywordEnabled(kw))
                        _previewMaterial.EnableKeyword(kw);

                    // Code speciffic to default unity shaders
                    if (_previewMaterial.globalIlluminationFlags == MaterialGlobalIlluminationFlags.EmissiveIsBlack)
                        _previewMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

                    if (_previewMaterial.GetColor("_EmissionColor") == Color.black)
                        _previewMaterial.SetColor("_EmissionColor", Color.white);
                }
                    
                _previewTextures = new Textures();

                _drag = new Vector2(35f, 35f);
            }
        }

        private void UpdatePreview()
        {
            if (_materia.MaterialData && _materia.MaterialData.Material)
            {
                if (_previewTextures == null || _previewMesh == null || _previewMaterial == null)
                    SetUpPreview();

                if (_previewTextures.Texs.Count != _materia.Properties.Count)
                {
                    _previewTextures.CreateTextures(_materia.Properties, 1, 1);
                    _previewTextures.SetTexturesToMaterial(_previewMaterial);
                }

                if (_materia.Properties.Count > 0)
                    _previewTextures.UpdateColors(SystemData.Settings.UVRect, _materia.Properties);
            }
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            _drag = EditorUtils.Drag2D(_drag, r);

            if (Event.current.type == EventType.Repaint)
            {
                _previewTexture = GeneratePreview(r, background, true);
                GUI.DrawTexture(r, _previewTexture, ScaleMode.ScaleToFit, false);
            }
        }

        private Texture GeneratePreview(Rect r, GUIStyle background, bool drag)
        {
            _previewRenderUtility.BeginPreview(r, background);
            _previewRenderUtility.DrawMesh(_previewMesh, Vector3.zero, new Vector3(1, 1, 1), Quaternion.identity, _previewMaterial, 0, null, null, false);
            _previewRenderUtility.camera.transform.position = Vector2.zero;

            if (drag)
                _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
            else
                _previewRenderUtility.camera.transform.rotation = Quaternion.identity;
            
            _previewRenderUtility.camera.transform.position = _previewRenderUtility.camera.transform.forward * -4f;
            _previewRenderUtility.camera.Render();

            return _previewRenderUtility.EndPreview();
        }

        private Texture2D GenerateThumbnail()
        {
            if (_materia.PreviewIcon != null)
                DestroyImmediate(_materia.PreviewIcon);

            var rt = (RenderTexture)GeneratePreview(new Rect(0f, 0f, 64f, 64f), GUIStyle.none, false);

            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);

            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = null;
            
            return tex;
        }

        public override void OnPreviewSettings()
        {
            if (GUILayout.Button(new GUIContent("Reset Camera")))
                _drag = new Vector2(35f, 35f);
        }

        public override bool HasPreviewGUI()
        {
            ValidateData();

            return true;
        }

        private void ValidateData()
        {
            if (_previewRenderUtility == null)
            {
                _previewRenderUtility = new PreviewRenderUtility();
                var camera = _previewRenderUtility.camera;

                camera.fieldOfView = 15f;
                camera.nearClipPlane = 2f;
                camera.transform.position = new Vector3(0f, 0f, -4f);
                camera.transform.rotation = Quaternion.identity;
                camera.renderingPath = RenderingPath.Forward;
                camera.useOcclusionCulling = false;

                _previewRenderUtility.lights[0].color = Color.white;
                _previewRenderUtility.lights[0].intensity = 0.5f;
                _previewRenderUtility.lights[1].intensity = 0.75f;
                _previewRenderUtility.lights[1].transform.rotation = Quaternion.Euler(new Vector3(20f, -175f, 0f));
            }
        }
        #endregion

        protected override void SetUpView()
        {
            var propCountBool = Convert.ToBoolean(_materia.Properties.Count);

            _createMateriaButton.visible = Convert.ToBoolean(_materialData.objectReferenceValue) && !propCountBool;
            _IMGUIContainer.visible = propCountBool;
            _restoreDefaultsButton.visible = propCountBool;
            _materialDataObjectField.SetEnabled(!propCountBool);
        }

        protected override void GetProperties()
        {
            _materialData = serializedObject.FindProperty("MaterialData");

            _description = serializedObject.FindProperty("_description");

            _IMGUIContainer = root.Q<IMGUIContainer>("IMGUIContainer");

            _materialDataObjectField = root.Q<ObjectField>("MaterialDataObjectField");
            _materialDataObjectField.objectType = typeof(MaterialData);

            _descriptionTextField = root.Q<TextField>("DescriptionTextField");

            _IMGUIContainer = root.Q<VisualElement>("IMGUIContainer");

            _createMateriaButton = root.Q<Button>("CreateMateriaButton");
            _restoreDefaultsButton = root.Q<Button>("RestoreDefaultsButton");
            _applyChangesButton = root.Q<Button>("ApplyChangesButton");

        }

        protected override void BindProperties()
        {
            _materialDataObjectField.BindProperty(_materialData);

            _descriptionTextField.BindProperty(_description);
        }

        protected override void RegisterCallbacks()
        {
            _createMateriaButton.clicked += CreateMateria;
            _restoreDefaultsButton.clicked += RestoreDefaults;
            _applyChangesButton.clicked += UpdateMateriaAtlases;

            _materialDataObjectField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(OnShaderDataChanged);
        }
    }
}