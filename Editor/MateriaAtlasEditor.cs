using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaAtlas))]
    public class MateriaAtlasEditor : MateriatorEditor
    {
        public MateriaAtlas MateriaAtlas;

        public MateriaSection MateriaSection;

        private SerializedProperty _materialData;
        private SerializedProperty _material;
        private SerializedProperty _materiaSlots;
        private SerializedProperty _format;
        private SerializedProperty _filterMode;

        public IMGUIContainer IMGUIContainer;
        private ObjectField _materialDataObjectField;
        private ObjectField _materialObjectField;
        private PropertyField _formatPropertyField;
        private PropertyField _filterModePropertyField;
        private Button _generateAtlasButton;

        private void OnEnable()
        {
            MateriaAtlas = (MateriaAtlas)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaAtlas>();

            IMGUIContainer defaultInspector = new IMGUIContainer(() => DrawDefaultInspector());
            //IMGUIContainer.Add(defaultInspector);

            MateriaSection = new MateriaSection(this, "MateriaSlots");

            return root;
        }

        private void GenerateAtlas()
        {
            if (_materialData.objectReferenceValue == null) return;
            if (_material.objectReferenceValue == null) return;
            if (_materiaSlots.arraySize < 1) return;

            AtlasFactory.GenerateAtlas(MateriaAtlas);
        }

        protected override void SetUpView()
        {

        }

        protected override void GetProperties()
        {
            _materialData = serializedObject.FindProperty("MaterialData");
            _material = serializedObject.FindProperty("Material");
            _materiaSlots = serializedObject.FindProperty("MateriaSlots");
            _format = serializedObject.FindProperty("Format");
            _filterMode = serializedObject.FindProperty("FilterMode");
        }

        protected override void BindProperties()
        {
            IMGUIContainer = root.Q<IMGUIContainer>("IMGUIContainer");

            _generateAtlasButton = root.Q<Button>("GenerateAtlasButton");

            _materialDataObjectField = root.Q<ObjectField>("MaterialDataObjectField");
            _materialDataObjectField.objectType = typeof(MaterialData);
            _materialDataObjectField.BindProperty(_materialData);

            _materialObjectField = root.Q<ObjectField>("MaterialObjectField");
            _materialObjectField.objectType = typeof(Material);
            _materialObjectField.BindProperty(_material);

            _formatPropertyField = root.Q<PropertyField>("FormatPropertyField");
            //_formatPropertyField.BindProperty(_format);

            _filterModePropertyField = root.Q<PropertyField>("FilterModePropertyField");
            _filterModePropertyField.BindProperty(_filterMode);

        }

        protected override void RegisterCallbacks()
        {
            _generateAtlasButton.clicked += GenerateAtlas;
        }
    }
}