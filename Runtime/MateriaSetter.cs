using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Materiator
{
    [DisallowMultipleComponent]
    public class MateriaSetter : MonoBehaviour
    {
        public bool IsDirty = true;

        public EditMode EditMode;

        public Renderer Renderer;
        public MeshFilter MeshFilter;
        public MeshRenderer MeshRenderer;
        public SkinnedMeshRenderer SkinnedMeshRenderer;

        public List<MateriaSlot> MateriaSlots;

        public Mesh Mesh;
        public Rect[] Rects;
        public SerializableDictionary<int, Rect> FilteredRects;

        public MateriaSetterData MateriaSetterData;

        public MateriaAtlas MateriaAtlas;
        public MateriaPreset MateriaPreset;
        public MaterialData MaterialData;

        public Material PreviousMaterial;
        public Material Material;
        public Textures Textures;

        public bool UseCustomGridSize;
        public Vector2Int GridSize;

        public Rect UVRect;


        public void Initialize()
        {
            Refresh();
        }

        public void Refresh()
        {
            GetMeshReferences();
            SetUpMaterial();
            SetUpGridSize();
            InitializeTextures();
            AnalyzeMesh();
            GenerateMateriaSlots();

            if (IsDirty)
                UpdateColorsOfAllTextures();
        }

        public void ResetMateriaSetter()
        {
            Renderer.sharedMaterial = null;

            var fields = GetType().GetFields();

            for (int i = 0; i < fields.Length; i++)
                fields[i].SetValue(this, null);

            Initialize();
        }

        public void GetMeshReferences()
        {
            Renderer = GetComponent<Renderer>();
            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();
            SkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

            if (MeshFilter == null)
            {
                if (SkinnedMeshRenderer != null)
                    Mesh = SkinnedMeshRenderer.sharedMesh;
            }
            else
            {
                Mesh = MeshFilter.sharedMesh;
            }
        }

        private void SetUpMaterial()
        {
            if (MaterialData == null)
                MaterialData = SystemData.Settings.DefaultMaterialData;

            if (Renderer != null)
            {
                if (Renderer.sharedMaterial == null || Material == null)
                {
                    Material = Instantiate(SystemData.Settings.DefaultMaterialData.Material);
                    Material.name = gameObject.name;
                }

                UpdateRenderer(false, true);
            }
        }

        public void UpdateRenderer(bool updateMesh = true, bool updateMaterial = true)
        {
            if (updateMesh)
            {
                if (MeshFilter != null)
                    MeshFilter.sharedMesh = Mesh;

                if (SkinnedMeshRenderer != null)
                    SkinnedMeshRenderer.sharedMesh = Mesh;
            }

            if (updateMaterial)
            {
                MaterialUtils.UpdateRendererMaterials(Renderer, Material, ref PreviousMaterial);
            }
        }

        private void SetUpGridSize()
        {
            if (EditMode == EditMode.Native)
            {
                if (!UseCustomGridSize)
                    GridSize = SystemData.Settings.DefaultGridSize;

                if (MateriaSetterData && !IsDirty)
                    GridSize = MateriaSetterData.NativeGridSize;
                
                UVRect = SystemData.Settings.UVRect;
            }
            else if (EditMode == EditMode.Atlas)
            {
                if (IsDirty)
                    GridSize = MateriaSetterData.NativeGridSize;
                else
                    GridSize = MateriaSetterData.MateriaAtlas.GridSize;
                
                UVRect = MateriaSetterData.AtlasedUVRect;
            }
        }

        private void InitializeTextures()
        {
            var shaderProps = MaterialData.ShaderData.MateriatorShaderProperties;

            if (Textures == null)
                Textures = new Textures();

            var gridSize = GridSize;
            if (EditMode == EditMode.Atlas)
                gridSize = MateriaSetterData.MateriaAtlas.GridSize;

            Textures.RemoveTextures(shaderProps, gridSize.x, gridSize.y);
            Textures.CreateTextures(shaderProps, gridSize.x, gridSize.y);
            
            SetTextures();
        }

        public void SetTextures()
        {
            if (Material == null) return;

            Textures.SetTexturesToMaterial(Material);

            // Is this the best place for this?
            foreach (var kw in MaterialData.ShaderData.Keywords)
            {
                if (!Material.IsKeywordEnabled(kw))
                {
                    Material.EnableKeyword(kw);
                }
            }
        }

        public void AnalyzeMesh()
        {
            if (MateriaSetterData != null && !IsDirty) // this here can probably be removed because it is being checked in SetUpGridSize fn
                GridSize = MateriaSetterData.NativeGridSize;

            Rects = MeshAnalyzer.CalculateRects(GridSize, UVRect);
            FilteredRects = MeshAnalyzer.FilterRects(Rects, Mesh.uv);
        }

        // TODO: Refactor this function (it is confusing)
        public void GenerateMateriaSlots()
        {
            var materiaSlotCount = 0;

            // Rebuild Materia Slots
            if (MateriaSlots != null)
            {
                var newMateriaSlots = new List<MateriaSlot>();

                foreach (var slot in MateriaSlots)
                {
                    if (FilteredRects.ContainsKey(slot.ID))
                    {
                        if (slot.Materia != null)
                            newMateriaSlots.Add(new MateriaSlot(slot.ID, slot.Materia, slot.Tag));
                        else
                            newMateriaSlots.Add(new MateriaSlot(slot.ID, SystemData.Settings.DefaultMateria, slot.Tag));
                    }
                }

                if (newMateriaSlots.Count != MateriaSlots.Count)
                    MateriaSlots = newMateriaSlots;

                materiaSlotCount = MateriaSlots.Count;

                if (!IsDirty && MateriaSetterData != null)
                    MateriaSlots = MateriaSetterData.MateriaSlots;
            }

            // If there are no slots or mesh was updated with extra UVs that led to more filtered rects than materia slots
            if (materiaSlotCount == 0 || FilteredRects.Count != materiaSlotCount)
            {
                if (MateriaSlots == null)
                    MateriaSlots = new List<MateriaSlot>();

                var materiaSlotIDs = new int[MateriaSlots.Count];

                for (int i = 0; i < materiaSlotIDs.Length; i++)
                    materiaSlotIDs[i] = MateriaSlots[i].ID;

                foreach (var rect in FilteredRects)
                    if (!materiaSlotIDs.Contains(rect.Key))
                        MateriaSlots.Add(new MateriaSlot(rect.Key));
            }
        }

        public void UpdateColorsOfAllTextures()
        {
            foreach (var rect in FilteredRects)
                Textures.UpdateColors(rect.Value, MateriaSlots.Where(ms => ms.ID == rect.Key).First().Materia.Properties);
        }

        public void LoadPreset(MateriaPreset preset)
        {
            if (preset == null) return;

            for (int i = 0; i < MateriaSlots.Count; i++)
                for (int j = 0; j < preset.MateriaPresetItems.Count; j++)
                    if (MateriaSlots[i].Tag.Name == preset.MateriaPresetItems[j].Tag.Name)
                        if (MateriaSlots[i].Materia != preset.MateriaPresetItems[j].Materia)
                            MateriaSlots[i].Materia = preset.MateriaPresetItems[j].Materia;
        }

        // Texture assigning needs to be figured out here
        public void LoadAtlas(MateriaAtlas atlas)
        {
            if (atlas != null)
            {
                EditMode = EditMode.Atlas;

                MateriaAtlas = atlas;

                Mesh = MateriaSetterData.AtlasedMesh;
                MaterialData = atlas.MaterialData;
                Material = atlas.Material;
                Textures = atlas.Textures;

                GridSize = MateriaSetterData.MateriaAtlas.GridSize;
                UVRect = MateriaSetterData.AtlasedUVRect;

                Textures.SetTexturesToMaterial(Material);
                UpdateRenderer();
            }
        }

        public void UnloadAtlas(bool removeAtlasData = false)
        {
            EditMode = EditMode.Native;

            if (removeAtlasData)
            {
                MateriaAtlas = null;
            }

            Mesh = MateriaSetterData.NativeMesh;
            MaterialData = MateriaSetterData.MaterialData;
            Material = MateriaSetterData.Material;
            Textures = MateriaSetterData.Textures;

            GridSize = MateriaSetterData.NativeGridSize;
            UVRect = SystemData.Settings.UVRect;

            Textures.SetTexturesToMaterial(Material);
            UpdateRenderer();
        }
    }
}