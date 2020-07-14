using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using System.Linq;

namespace Materiator
{
    [CustomEditor(typeof(Materia))]
    [CanEditMultipleObjects]
    public class MateriaEditor : MateriatorEditor
    {
        private Materia _materia;

        private VisualElement _IMGUIContainer;

        private SerializedProperty _baseColor;
        private SerializedProperty _metallic;
        private SerializedProperty _smoothness;
        private SerializedProperty _isEmissive;
        private SerializedProperty _emissionColor;

        private ColorField _baseColorField;
        private Toggle _isEmissiveToggle;
        private ColorField _emissionColorField;

        private PreviewRenderUtility _previewRenderUtility;
        private Mesh _previewMesh;
        private Material _previewMaterial;
        private Texture _previewTexture;
        private Vector2 _drag;

        private void OnEnable()
        {
            _materia = (Materia)target;

            SetUpPreview();

            OnValueChanged();
        }

        private void OnDisable()
        {
            _previewRenderUtility?.Cleanup();
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<Materia>();

            _baseColorField.RegisterCallback<ChangeEvent<Color>>(e =>
            {
                OnValueChanged();
            });

            DrawEmissionSection();

            _emissionColorField.RegisterCallback<ChangeEvent<Color>>(e =>
            {
                OnValueChanged();
            });

            IMGUIContainer defaultInspector = new IMGUIContainer(() => IMGUI());
            _IMGUIContainer.Add(defaultInspector);

            return root;
        }

        private void DrawEmissionSection()
        {
            if (_isEmissiveToggle.value)
            {
                _emissionColorField.SetEnabled(true);
            }
            else
            {
                _emissionColorField.SetEnabled(false);
            }

            _isEmissiveToggle.RegisterCallback<ChangeEvent<bool>>(e =>
            {
                if (e.newValue)
                {
                    _emissionColorField.SetEnabled(true);
                }
                else
                {
                    _emissionColorField.SetEnabled(false);
                }

                OnValueChanged();
            });
        }

        private void IMGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_metallic);
            EditorGUILayout.PropertyField(_smoothness);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                OnValueChanged();
            }
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (_materia == null || _materia.PreviewIcon == null)
                return null;

            Texture2D staticPreviewTex = new Texture2D(width, height);
            EditorUtility.CopySerialized(_materia.PreviewIcon, staticPreviewTex);

            return staticPreviewTex;
        }

        private void OnValueChanged()
        {
            UpdateSceneMateriaSettersColors();
            UpdatePrefabMateriaSettersColors();
            EditorUtils.GenerateMateriaPreviewIcons(_materia, _previewMaterial);
        }

        private void UpdateSceneMateriaSettersColors()
        {
            var materiaSetters = GameObject.FindObjectsOfType<MateriaSetter>();

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                var rootGameObjects = prefabStage.scene.GetRootGameObjects();
                for (int i = 0; i < rootGameObjects.Length; i++)
                {
                    var obj = rootGameObjects[i].GetComponent<MateriaSetter>();
                    if (obj != null) obj.Refresh();
                }
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }

            for (int i = 0; i < materiaSetters.Length; i++)
            {
                var aaa = materiaSetters[i].MateriaSlots.Where(m => m.Materia == (Materia)target);
                if (aaa.Count() != 0)
                {
                    materiaSetters[i].UpdateColorsOfAllTextures();
                }
            }
        }

        private void UpdatePrefabMateriaSettersColors()
        {
            var materiaSetters = AssetUtils.FindAllComponentsInPrefabs<MateriaSetter>();

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                var rootGameObjects = prefabStage.scene.GetRootGameObjects();
                for (int i = 0; i < rootGameObjects.Length; i++)
                {
                    var obj = rootGameObjects[i].GetComponent<MateriaSetter>();
                    if (obj != null) obj.Refresh();
                }
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }

            for (int i = 0; i < materiaSetters.Count; i++)
            {
                var aaa = materiaSetters[i].MateriaSlots.Where(m => m.Materia == (Materia)target);
                if (aaa.Count() != 0)
                {
                    materiaSetters[i].UpdateColorsOfAllTextures();
                }
            }
        }

        #region Preview
        private void SetUpPreview()
        {
            _previewMesh = AssetUtils.LoadAssetFromUniqueAssetPath<Mesh>("Library/unity default resources::Sphere");
            _previewMaterial = new Material(SystemData.Settings.DefaultShaderData.Shader);

            _drag = new Vector2(35f, 35f);
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            _drag = EditorUtils.Drag2D(_drag, r);

            _previewRenderUtility.BeginPreview(r, background);

            _previewRenderUtility.DrawMesh(_previewMesh, Vector3.zero, new Vector3(1, 1, 1), Quaternion.identity, _previewMaterial, 0, null, null, false);

            _previewRenderUtility.camera.transform.position = Vector2.zero;
            _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
            _previewRenderUtility.camera.transform.position = _previewRenderUtility.camera.transform.forward * -4f;
            _previewRenderUtility.camera.Render();

            _previewTexture = _previewRenderUtility.EndPreview();
            GUI.DrawTexture(r, _previewTexture, ScaleMode.ScaleToFit, false);
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
            }
        }
#endregion

        protected override void GetProperties()
        {
            _baseColor = serializedObject.FindProperty("BaseColor");
            _metallic = serializedObject.FindProperty("Metallic");
            _smoothness = serializedObject.FindProperty("Smoothness");
            _isEmissive = serializedObject.FindProperty("IsEmissive");
            _emissionColor = serializedObject.FindProperty("EmissionColor");

            _IMGUIContainer = root.Q<IMGUIContainer>("IMGUIContainer");

            _baseColorField = root.Q<ColorField>("BaseColorField");
            _isEmissiveToggle = root.Q<Toggle>("IsEmissiveToggle");
            _emissionColorField = root.Q<ColorField>("EmissionColorField");

            _IMGUIContainer = root.Q<VisualElement>("IMGUIContainer");
        }

        protected override void BindProperties()
        {
            _baseColorField.BindProperty(_baseColor);
            _isEmissiveToggle.BindProperty(_isEmissive);
            _emissionColorField.BindProperty(_emissionColor);
        }

        protected override void RegisterButtons()
        {

        }
    }
}