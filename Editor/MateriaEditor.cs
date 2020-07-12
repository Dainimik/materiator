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

        //private PreviewRenderUtility _previewRenderUtility;
        //private Mesh _mesh;
        //private Material _material;
        //private Vector2 _drag;
        //private Texture _previewTexture;

        private void OnEnable()
        {
            _materia = (Materia)target;
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

        private void OnValueChanged()
        {
            UpdateSceneMateriaSettersColors();
            UpdatePrefabMateriaSettersColors();
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