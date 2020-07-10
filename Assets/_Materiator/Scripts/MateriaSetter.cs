using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Materiator
{
    public enum EditMode
    {
        Native = 0,
        Atlas = 1
    }

    public class MateriaSetter : MonoBehaviour
    {
        public bool IsInitialized = false;
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

        public Mesh NativeMesh;
        public Mesh AtlasedMesh;

        public int GridSize;
        public Rect UVRect;


        public void Initialize()
        {
            IsInitialized = false;

            Refresh();

            IsInitialized = true;
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
            {
                fields[i].SetValue(this, null);
            }

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
                {
                    Mesh = SkinnedMeshRenderer.sharedMesh;
                }
            }
            else
            {
                Mesh = MeshFilter.sharedMesh;
            }

            if (EditMode == EditMode.Native)
            {
                NativeMesh = Mesh;
            }
        }

        private void SetUpRenderer()
        {
            if (MaterialData == null)
                MaterialData = SystemData.Settings.DefaultMaterialData;

            if (Renderer.sharedMaterial == null || (Renderer.sharedMaterial != null && Material == null))
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
                UVRect = new Rect(0f, 0f, 1f, 1f);
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
            {
                Textures = new Textures();
            }

            if (Textures.Color == null || Textures.MetallicSmoothness == null || Textures.Emission == null)
            {
                Textures.CreateTextures(GridSize, GridSize);
            }

            SetTextures();
        }

        public void SetTextures()
        {
            if (Material == null) return;

            Textures.SetTextures(Material, MaterialData.ShaderData);
        }

        public void AnalyzeMesh()
        {
            var gridSize = SystemData.Settings.GridSize;

            if (MateriaSetterData != null)
            {
                gridSize = MateriaSetterData.NativeGridSize;
            }

            Rects = MeshAnalyzer.CalculateRects(gridSize, UVRect);
            FilteredRects = MeshAnalyzer.FilterRects(Rects, Mesh.uv);
        }

        public void GenerateMateriaSlots(bool reset = false, bool refresh = false)
        {
            var materiaSlotsCount = 0;

            if (MateriaSlots != null)
            {
                var newMateriaSlots = new List<MateriaSlot>();

                foreach (var ms in MateriaSlots)
                {
                    if (FilteredRects.ContainsKey(ms.ID))
                    {
                        if (ms.Materia != null)
                        {
                            newMateriaSlots.Add(new MateriaSlot(ms.ID, ms.Materia, ms.Tag));
                        }
                        else
                        {
                            newMateriaSlots.Add(new MateriaSlot(ms.ID, SystemData.Settings.DefaultMateria, ms.Tag));
                        }
                    }
                }

                if (newMateriaSlots.Count != MateriaSlots.Count)
                {
                    MateriaSlots = newMateriaSlots;
                }

                materiaSlotsCount = MateriaSlots.Count;

                if (!IsDirty && MateriaSetterData != null)
                {
                    MateriaSlots = MateriaSetterData.MateriaSlots;
                }
            }

            if (reset || refresh || materiaSlotsCount == 0 || FilteredRects.Count != materiaSlotsCount)
            {
                if (MateriaSlots == null)
                {
                    MateriaSlots = new List<MateriaSlot>();
                }

                if (reset)
                {
                    if (MateriaSlots != null)
                    {
                        MateriaSlots.Clear(); 
                    }
                    else
                    {
                        MateriaSlots = new List<MateriaSlot>();
                    }
                }

                var keyArray = new int[MateriaSlots.Count];
                for (int i = 0; i < keyArray.Length; i++)
                {
                    keyArray[i] = MateriaSlots[i].ID;
                }

                foreach (var rect in FilteredRects)
                {
                    if (!keyArray.Contains(rect.Key))
                    {
                        MateriaSlots.Add(new MateriaSlot(rect.Key));
                    }
                }
            }
        }

        public void UpdateColorsOfAllTextures()
        {
            Textures.UpdateColors(FilteredRects, GridSize, MateriaSlots);
        }

        public void LoadPreset(MateriaPreset preset)
        {
            Materia materia;

            if (preset != null)
            {
                for (int i = 0; i < MateriaSlots.Count; i++)
                {
                    for (int j = 0; j < preset.MateriaPresetItemList.Count; j++)
                    {
                        if (MateriaSlots[i].Tag == preset.MateriaPresetItemList[j].Tag)
                        {
                            if (MateriaSetterData != null)
                            {
                                materia = MateriaSetterData.MateriaSlots[i].Materia;
                            }
                            else
                            {
                                materia = MateriaSlots[i].Materia;
                            }

                            if (materia != preset.MateriaPresetItemList[j].Materia)
                            {
                                MateriaSlots[i].Materia = preset.MateriaPresetItemList[j].Materia;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < MateriaSlots.Count; i++)
                {
                    MateriaSlots[i].Materia = SystemData.Settings.DefaultMateria;
                }
            }
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
                Textures = atlas.Textures;
                
                GridSize = MateriaSetterData.AtlasedGridSize;
                UVRect = MateriaSetterData.AtlasedUVRect;

                Textures.SetTextures(Material, MaterialData.ShaderData);
                UpdateRenderer();
            }
        }

        public void UnloadAtlas()
        {
            EditMode = EditMode.Native;

            Mesh = MateriaSetterData.NativeMesh;
            MaterialData = MateriaSetterData.MaterialData;
            Material = MateriaSetterData.Material;
            Textures = MateriaSetterData.Textures;

            GridSize = MateriaSetterData.NativeGridSize;
            UVRect = new Rect(0f, 0f, 1f, 1f);

            Textures.SetTextures(Material, MaterialData.ShaderData);
            UpdateRenderer();
        }
    }
}