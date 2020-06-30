using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Materiator
{
    public enum EditMode
    {
        Nascent,
        Atlas
    }

    public class MateriaSetter : MonoBehaviour
    {
        public bool IsInitialized = false;
        public bool IsDirty = true;
        public EditMode EditMode;

        public MateriaSetterData MateriaSetterData;
        public Mesh Mesh;
        public MeshFilter MeshFilter;
        public Renderer Renderer;
        public MeshRenderer MeshRenderer;
        public SkinnedMeshRenderer SkinnedMeshRenderer;

        public List<MateriaSlot> MateriaSlots;

        public SerializableDictionary<int, Rect> FilteredRects;
        public Rect[] Rects;

        public MateriaAtlas MateriaAtlas;
        public MateriaPreset MateriaPreset;
        public ShaderData ShaderData;
        public Material Material;

        public Textures Textures;

        public Mesh OriginalMesh;
        public Mesh AtlasedMesh;
        public int OriginalGridSize;
        public int AtlasedGridSize;
        public int GridSize;
        public Rect AtlasedUVRect;


        public void Initialize()
        {
            IsInitialized = false;

            GetMeshReferences();
            SetUpRenderer();
            InitializeTextures();
            AnalyzeMesh();
            GenerateMateriaSlots(true);

            IsInitialized = true;
        }

        public void Refresh()
        {
            GetMeshReferences();
            SetUpRenderer();
            InitializeTextures();
            AnalyzeMesh();
            GenerateMateriaSlots(false);
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

        private void GetMeshReferences()
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
        }

        private void SetUpRenderer()
        {
            if (ShaderData == null)
                ShaderData = Utils.Settings.DefaultShaderData;

            if (Renderer.sharedMaterial == null)
            {
                Material = Utils.CreateMaterial(ShaderData.Shader, gameObject.name);
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

        private void InitializeTextures()
        {
            if (Textures == null)
            {
                Textures = new Textures();
            }

            if (Textures.Color == null || Textures.MetallicSmoothness == null || Textures.Emission == null)
            {
                if (OriginalGridSize == 0)
                {
                    // why do i need this many gridSize variables?
                    OriginalGridSize = Utils.Settings.GridSize;
                    GridSize = OriginalGridSize;
                }

                Textures.CreateTextures(GridSize, GridSize);
            }

            SetTextures();
        }

        public void SetTextures()
        {
            if (Material == null) return;

            Textures.SetTextures(Material, ShaderData);
        }

        private void AnalyzeMesh()
        {
            var uvPositionRect = new Rect(0f, 0f, 1f, 1f);

            if (EditMode == EditMode.Nascent)
            {
                GridSize = OriginalGridSize;
            }
            if (EditMode == EditMode.Atlas)
            {
                GridSize = AtlasedGridSize;
                uvPositionRect = AtlasedUVRect;
            }

            Rects = MeshAnalyzer.CalculateRects(GridSize, uvPositionRect);
            FilteredRects = MeshAnalyzer.FilterRects(Rects, Mesh.uv);
        }

        public void GenerateMateriaSlots(bool reset)
        {
            var materiaSlotsCount = 0;

            if (MateriaSlots != null)
            {
                materiaSlotsCount = MateriaSlots.Count;
            }

            if (reset || materiaSlotsCount == 0 || FilteredRects.Count != materiaSlotsCount)
            {
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

        public void LoadPreset(MateriaPreset preset, out int numberOfSameMateria)
        {
            numberOfSameMateria = 0;
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

                            if (materia == preset.MateriaPresetItemList[j].Materia)
                            {
                                numberOfSameMateria++;
                            }
                            else
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
                    MateriaSlots[i].Materia = Utils.Settings.DefaultMateria;
                }
            }
        }

        public void LoadAtlas(MateriaAtlas atlas)
        {
            if (atlas != null)
            {
                EditMode = EditMode.Atlas;

                Utils.ShallowCopyFields(ShaderData, this);

                ShaderData = atlas.ShaderData;
                Material = atlas.Material;
                Textures = atlas.Textures;
                Mesh = MateriaSetterData.AtlasedMesh;

                Textures.SetTextures(Material, ShaderData);
                UpdateRenderer();
                GenerateMateriaSlots(true);
            }
        }

        public void UnloadAtlas(MateriaAtlas atlas)
        {
            if (atlas != null)
            {
                EditMode = EditMode.Nascent;

                ShaderData = atlas.ShaderData;
                Material = atlas.Material;
                Textures = atlas.Textures;
                Mesh = MateriaSetterData.AtlasedMesh;

                Textures.SetTextures(Material, ShaderData);
                UpdateRenderer();
                GenerateMateriaSlots(true);
            }
        }
    }
}