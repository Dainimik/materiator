using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaSetter))]
    public class MateriaSetterEditor : MateriatorEditor
    {
        public MateriaSetter MateriaSetter;

        public Action<bool> OnDirtyChanged;
        public Action OnMateriaSetterUpdated;

        public SerializedProperty EditMode;
        public SerializedProperty IsDirty;
        public SerializedProperty Material;

        public SerializedProperty UseCustomGridSize;
        public SerializedProperty GridSize;

        public VisualElement Root;

        public AtlasSection AtlasSection;
        public PresetSection PresetSection;
        public DataSection DataSection;
        public OutputSection OutputSection;

        private Button _reloadButton;

        private SerializedProperty _materiaSetterData;

        private void OnEnable()
        {
            MateriaSetter = (MateriaSetter)target;

            InitializeEditor<MateriaSetter>();

            Root = new VisualElement();

            if (Initialize())
            {
                AtlasSection = new AtlasSection(this);
                PresetSection = new PresetSection(this);
                DataSection = new DataSection(this);
                OutputSection = new OutputSection(this);

                DrawDefaultGUI();
            }

            // Temp for debugging
            if (MateriaSetter.Textures.ID == 0)
            {
                MateriaSetter.Textures.ID = UnityEngine.Random.Range(-999999, 9999999);
            }
        }

        private void DrawDefaultGUI()
        {
            IMGUIContainer defaultInspector = new IMGUIContainer(() => DrawDefaultInspector());
            root.Add(defaultInspector);
        }

        public override VisualElement CreateInspectorGUI()
        {
            return Root;
        }

        public bool Initialize()
        {
            if (SystemChecker.CheckAllSystems(this))
            {
                MateriaSetter.Initialize();

                Root = root;

                return true;
            }
            else
            {
                return false;
            }
        }

        private void Refresh()
        {
            MateriaSetter.Refresh();           
        }

        public void ResetMateriaSetter()
        {
            MateriaSetter.ResetMateriaSetter();

            SetMateriaSetterDirty(true);
        }

        public void SetMateriaSetterDirty(bool value)
        {
            serializedObject.Update();
            if (value)
            {
                if (!IsDirty.boolValue)
                {
                    IsDirty.boolValue = true;

                    CreateEditModeData(EditMode.enumValueIndex);
                }
            }
            else
            {
                if (IsDirty.boolValue)
                {
                    IsDirty.boolValue = false;
                }  
            }

            serializedObject.ApplyModifiedProperties();

            OnDirtyChanged?.Invoke(value);
        }

        private void CreateEditModeData(int editMode)
        {
            if (_materiaSetterData.objectReferenceValue != null)
            {
                var newTextures = new Textures();

                Textures sourceTextures = null;

                if (editMode == 0)
                {
                    sourceTextures = MateriaSetter.MateriaSetterData.Textures;
                }
                else if (editMode == 1)
                {
                    sourceTextures = MateriaSetter.MateriaSetterData.MateriaAtlas.Textures;
                }

                newTextures.CreateTextures(MateriaSetter.MaterialData.ShaderData.Properties, sourceTextures.Size.x, sourceTextures.Size.y);
                var mat = Instantiate(Material.objectReferenceValue);
                MateriaSetter.UpdateColorsOfAllTextures();

                if (name != null)
                    mat.name = name;

                Material.objectReferenceValue = mat;
                serializedObject.ApplyModifiedProperties(); // This should be after assigning newTextures to Textures or else editing Setter prefab in project view fails! But then any materia modification will apply changes to texture immediately

                MateriaSetter.Textures = newTextures;
                MateriaSetter.SetTextures();

                var newMateriaSlots = new List<MateriaSlot>();

                foreach (var item in MateriaSetter.MateriaSetterData.MateriaSlots)
                    newMateriaSlots.Add(new MateriaSlot(item.ID, item.Materia, item.Tag));

                MateriaSetter.MateriaSlots = newMateriaSlots;

                MateriaSetter.UpdateRenderer();
            }
        }

        protected override void GetProperties()
        {
            EditMode = serializedObject.FindProperty("EditMode");
            IsDirty = serializedObject.FindProperty("IsDirty");
            Material = serializedObject.FindProperty("Material");

            UseCustomGridSize = serializedObject.FindProperty("UseCustomGridSize");
            GridSize = serializedObject.FindProperty("GridSize");

            _materiaSetterData = serializedObject.FindProperty("MateriaSetterData");

            _reloadButton = root.Q<Button>("ReloadButton");
        }

        protected override void SetUpView()
        {
            //
        }

        protected override void BindProperties()
        {
            //
        }

        protected override void RegisterCallbacks()
        {
            _reloadButton.clicked += Refresh;
        }
    }
}
