using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Materiator
{
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

        public Material Material;
        public Textures Textures;

        public Vector2Int GridSize;
        public Rect UVRect;


        public void Initialize()
        {
            Refresh();
        }

        public void Refresh()
        {
            GetMeshReferences();
            SetUpRenderer();
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
            IsDirty = true;
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

        private void SetUpRenderer()
        {
            if (MaterialData == null)
                MaterialData = SystemData.Settings.DefaultMaterialData;

            if (Renderer != null)
            {
                if (Renderer.sharedMaterial == null || Material == null)
                {
                    Material = Instantiate(SystemData.Settings.DefaultMaterialData.Material);
                    Material.name = gameObject.name;
                    UpdateRenderer(false);
                }
                else
                {
                    Material = Renderer.sharedMaterial;
                }
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
                Renderer.sharedMaterial = Material;
        }

        private void SetUpGridSize()
        {
            if (EditMode == EditMode.Native)
            {
                GridSize = SystemData.Settings.GridSize;
                UVRect = SystemData.Settings.UVRect;
            }
            else if (EditMode == EditMode.Atlas)
            {
                GridSize = MateriaSetterData.AtlasedGridSize;
                UVRect = MateriaSetterData.AtlasedUVRect;
            }
        }

        private void InitializeTextures()
        {
            if (Textures == null)
                Textures = new Textures();

            if (Textures.Color == null || Textures.MetallicSmoothness == null || Textures.Emission == null)
                Textures.CreateTextures(GridSize.x, GridSize.y);

            SetTextures();
        }

        public void SetTextures()
        {
            if (Material == null) return;

            Textures.SetTexturesToMaterial(Material, MaterialData.ShaderData);
        }

        public void AnalyzeMesh()
        {
            var gridSize = SystemData.Settings.GridSize;

            if (MateriaSetterData != null)
                gridSize = MateriaSetterData.NativeGridSize;

            Rects = MeshAnalyzer.CalculateRects(gridSize, UVRect);
            FilteredRects = MeshAnalyzer.FilterRects(Rects, Mesh.uv);
        }

        // TODO: Refactor this function (it is confusing)
        public void GenerateMateriaSlots()
        {
            var materiaSlotsCount = 0;

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

                materiaSlotsCount = MateriaSlots.Count;

                if (!IsDirty && MateriaSetterData != null)
                    MateriaSlots = MateriaSetterData.MateriaSlots;
            }

            // If there are no slots or mesh was updated with extra UVs that led to more filtered rects than materia slots
            if (materiaSlotsCount == 0 || FilteredRects.Count != materiaSlotsCount)
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
            Textures.UpdateColors(FilteredRects, GridSize, MateriaSlots);
        }

        public void LoadPreset(MateriaPreset preset)
        {
            if (preset == null) return;

            for (int i = 0; i < MateriaSlots.Count; i++)
                for (int j = 0; j < preset.MateriaPresetItemList.Count; j++)
                    if (MateriaSlots[i].Tag.Name == preset.MateriaPresetItemList[j].Tag.Name)
                        if (MateriaSlots[i].Materia != preset.MateriaPresetItemList[j].Materia)
                            MateriaSlots[i].Materia = preset.MateriaPresetItemList[j].Materia;
        }

        public void LoadAtlas(MateriaAtlas atlas)
        {
            if (atlas != null)
            {
                EditMode = EditMode.Atlas;

                MateriaAtlas = atlas;

                Mesh = MateriaSetterData.AtlasedMesh;
                MaterialData = atlas.MaterialData;
                Material = atlas.Material;
                Textures.Assign(atlas.Textures);

                GridSize = MateriaSetterData.AtlasedGridSize;
                UVRect = MateriaSetterData.AtlasedUVRect;

                Textures.SetTexturesToMaterial(Material, MaterialData.ShaderData);
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
            Textures.Assign(MateriaSetterData.Textures);

            GridSize = MateriaSetterData.NativeGridSize;
            UVRect = SystemData.Settings.UVRect;

            Textures.SetTexturesToMaterial(Material, MaterialData.ShaderData);
            UpdateRenderer();
        }
    }
}