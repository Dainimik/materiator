using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using System.Linq;
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

        private SerializedProperty _shaderData;
        private SerializedProperty _description;

        private ReorderableList _materiaPropertyList;

        private ObjectField _shaderDataObjectField;
        private TextField _descriptionTextField;

        private Button _createMateriaButton;
        private Button _restoreDefaultsButton;

        private PreviewRenderUtility _previewRenderUtility;
        private Mesh _previewMesh;
        private Material _previewMaterial;
        private Textures _previewTextures;
        private Texture _previewTexture;
        private Vector2 _drag;

        private void OnEnable()
        {
            _materia = (Materia)target;

            SetUpPreview();
            UpdatePreview(_previewMaterial);
                

            //EditorUtils.GenerateMateriaPreviewIcons(_materia, _previewMaterial);

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

            // This should not be here
            if (_materia.Properties.Count > 0)
            {
                _shaderDataObjectField.SetEnabled(false);
            }

            return root;
        }

        private void IMGUI()
        {
            serializedObject.Update();
            
            _materiaPropertyList.DoLayoutList();           

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateMateria()
        {
            var shaderData = _materia.ShaderData;
            if (!shaderData) return;

            var shaderDataProperties = _materia.ShaderData.Properties;

            _materia.Properties.Clear();
            _materia.AddProperties(shaderDataProperties);

            OnValueChanged();

            // TODO: This should not be here
            _shaderDataObjectField.SetEnabled(false);

            SetUpView();
        }

        private void RestoreDefaults()
        {
            if (EditorUtility.DisplayDialog("Restore Defaults", "All current settings will be lost. Are you sure?", "Yes", "No"))
            {
                CreateMateria();
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
                var element = _materiaPropertyList.serializedProperty.GetArrayElementAtIndex(index);

                Rect r = new Rect(rect.x, rect.y, rect.width, rect.height);

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(r, element);
                serializedObject.Update();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    OnValueChanged();
                }
            };

            _materiaPropertyList.elementHeightCallback = (int index) =>
            {
                var element = _materiaPropertyList.serializedProperty.GetArrayElementAtIndex(index);
                float propertyHeight = EditorGUI.GetPropertyHeight(element, true);
                float spacing = EditorGUIUtility.singleLineHeight;

                if (element.managedReferenceFullTypename == "")
                {
                    spacing += 20;
                }

                return propertyHeight + spacing;
            };

            /*_materiaPropertyList.onAddCallback = (ReorderableList list) =>
            {
                var index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            };*/
        }

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            if (_materia == null || _materia.PreviewIcon == null)
                return null;

            Texture2D staticPreviewTex = new Texture2D(width, height);
            EditorUtility.CopySerialized(_materia.PreviewIcon, staticPreviewTex);

            return staticPreviewTex;
        }

        private void OnValueChanged()
        {
            UpdatePreview(_previewMaterial);
            UpdateSceneMateriaSettersColors();
            UpdatePrefabMateriaSettersColors();
        }

        private void UpdateSceneMateriaSettersColors()
        {
            var materiaSetters = GameObject.FindObjectsOfType<MateriaSetter>();

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

        private void OnShaderDataChanged(ChangeEvent<UnityEngine.Object> e)
        {
            _shaderData.objectReferenceValue = e.newValue;
            serializedObject.ApplyModifiedProperties();

            SetUpView();
        }

        #region Preview
        private void SetUpPreview()
        {
            if (_materia.ShaderData)
            {
                _previewMesh = AssetUtils.LoadAssetFromUniqueAssetPath<Mesh>("Library/unity default resources::Sphere");
                _previewMaterial = new Material(_materia.ShaderData.Shader);
                _previewTextures = new Textures();

                _drag = new Vector2(35f, 35f);
            }
        }

        private void UpdatePreview(Material material)
        {
            if (_materia.ShaderData)
            {
                if (_previewTextures == null)
                    SetUpPreview();

                if (_previewTextures.Texs.Count != _materia.Properties.Count)
                {
                    _previewTextures.CreateTextures(_materia.Properties, 4, 4);
                    _previewTextures.SetTexturesToMaterial(_previewMaterial);
                }

                if (_materia.Properties.Count > 0)
                    _previewTextures.UpdateColor(_materia.Properties);
            }
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            _drag = EditorUtils.Drag2D(_drag, r);

            if (Event.current.type == EventType.Repaint)
            {
                _previewRenderUtility.BeginPreview(r, background);

                _previewRenderUtility.DrawMesh(_previewMesh, Vector3.zero, new Vector3(1, 1, 1), Quaternion.identity, _previewMaterial, 0, null, null, false);

                _previewRenderUtility.camera.transform.position = Vector2.zero;
                _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
                _previewRenderUtility.camera.transform.position = _previewRenderUtility.camera.transform.forward * -4f;
                _previewRenderUtility.camera.Render();

                _previewTexture = _previewRenderUtility.EndPreview();
                GUI.DrawTexture(r, _previewTexture, ScaleMode.ScaleToFit, false);
            }
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
            _createMateriaButton.visible = (Convert.ToBoolean(_shaderData.objectReferenceValue) && Convert.ToBoolean(_materia.Properties.Count == 0));

            _IMGUIContainer.visible = Convert.ToBoolean(_materia.Properties.Count > 0);

            _restoreDefaultsButton.visible = Convert.ToBoolean(_materia.Properties.Count);
        }

        protected override void GetProperties()
        {
            _shaderData = serializedObject.FindProperty("ShaderData");

            _description = serializedObject.FindProperty("_description");

            _IMGUIContainer = root.Q<IMGUIContainer>("IMGUIContainer");

            _shaderDataObjectField = root.Q<ObjectField>("ShaderDataObjectField");
            _shaderDataObjectField.objectType = typeof(ShaderData);

            _descriptionTextField = root.Q<TextField>("DescriptionTextField");

            _IMGUIContainer = root.Q<VisualElement>("IMGUIContainer");

            _createMateriaButton = root.Q<Button>("CreateMateriaButton");
            _restoreDefaultsButton = root.Q<Button>("RestoreDefaultsButton");

        }

        protected override void BindProperties()
        {
            _shaderDataObjectField.BindProperty(_shaderData);

            _descriptionTextField.BindProperty(_description);
        }

        protected override void RegisterCallbacks()
        {
            _createMateriaButton.clicked += CreateMateria;
            _restoreDefaultsButton.clicked += RestoreDefaults;

            _shaderDataObjectField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(OnShaderDataChanged);
        }
    }
}