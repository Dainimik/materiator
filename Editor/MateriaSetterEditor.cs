﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
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

        private Button _reloadButton;

        public VisualElement Root;

        public UVInspector UVInspector;

        public AtlasSection AtlasSection;
        public PresetSection PresetSection;
        public DataSection DataSection;
        public OutputSection OutputSection;

        private void OnEnable()
        {
            MateriaSetter = (MateriaSetter)target;

            InitializeEditor<MateriaSetter>();

            Root = new VisualElement();

            if (Initialize())
            {
                UVInspector = new UVInspector(MateriaSetter, Root);

                AtlasSection = new AtlasSection(this);
                PresetSection = new PresetSection(this);
                DataSection = new DataSection(this);
                OutputSection = new OutputSection(this);

                DrawDefaultGUI();

                RegisterCallbacks();
            }

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

        private void OnDisable()
        {
            MateriaSetter = (MateriaSetter)target;

            UnregisterCallbacks();
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
            UVInspector.DrawUVInspector(true);

            var shader = MateriaSetter.MaterialData.ShaderData.Shader;

            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                Debug.Log("Name: " + ShaderUtil.GetPropertyName(shader, i) + "     Type: " + ShaderUtil.GetPropertyType(shader, i));
            }
            
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

            UVInspector.DrawUVInspector(true);

            serializedObject.ApplyModifiedProperties();

            OnDirtyChanged?.Invoke(value);
        }

        private void CreateEditModeData(int editMode)
        {
            if (MateriaSetter.MateriaSetterData != null)
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
                newTextures.CopyPixelColors(sourceTextures, sourceTextures.Size, SystemData.Settings.UVRect, newTextures.Size, SystemData.Settings.UVRect);

                if (name != null)
                    mat.name = name;

                Material.objectReferenceValue = mat;

                serializedObject.ApplyModifiedProperties();

                MateriaSetter.Textures = newTextures;
                MateriaSetter.SetTextures();

                var newMateriaSlots = new List<MateriaSlot>();

                foreach (var item in MateriaSetter.MateriaSetterData.MateriaSlots)
                {
                    newMateriaSlots.Add(new MateriaSlot(item.ID, item.Materia, item.Tag));
                }

                MateriaSetter.MateriaSlots = newMateriaSlots;
                serializedObject.Update(); // Why are two updates here necessary?
                MateriaSetter.UpdateRenderer();
                serializedObject.Update(); // Why are two updates here necessary?
            }
        }

        private void RegisterCallbacks()
        {
            PresetSection.OnPresetLoaded += () => UVInspector.DrawUVInspector(true);
        }

        private void UnregisterCallbacks()
        {
            if (UVInspector != null)
                PresetSection.OnPresetLoaded -= () => UVInspector.DrawUVInspector(true);
        }

        protected override void GetProperties()
        {
            EditMode = serializedObject.FindProperty("EditMode");
            IsDirty = serializedObject.FindProperty("IsDirty");
            Material = serializedObject.FindProperty("Material");

            _reloadButton = root.Q<Button>("ReloadButton");
        }

        protected override void RegisterButtons()
        {
            _reloadButton.clicked += Refresh;
        }
    }
}
