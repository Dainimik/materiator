using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    [DisallowMultipleComponent]
    public class MateriaSetter : MonoBehaviour
    {
        public bool IsDirty = true;

        public Renderer Renderer;
        public MeshFilter MeshFilter;
        public MeshRenderer MeshRenderer;
        public SkinnedMeshRenderer SkinnedMeshRenderer;

        public List<MateriaSetterSlot> MateriaSetterSlots;

        public Mesh Mesh;

        public MateriaAtlas MateriaAtlas;

        public Material Material;
        public Textures Textures;

        public void Initialize()
        {
            Refresh();
        }

        public void Refresh()
        {
            GetMeshReferences();
            //SetUpMaterial();
            //SetUpGridSize();
            //AnalyzeMesh();
            //GenerateMateriaSlots();

            //if (IsDirty)
                //UpdateColorsOfAllTextures();
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
            //if (MaterialData == null)
             //   MaterialData = SystemData.Settings.DefaultMaterialData;

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
        }

        private void SetUpGridSize()
        {

        }

        public void AnalyzeMesh()
        {
            //if (MateriaSetterData != null && !IsDirty) // this here can probably be removed because it is being checked in SetUpGridSize fn
             //   GridSize = MateriaSetterData.NativeGridSize;

            //Rects = MeshAnalyzer.CalculateRects(GridSize, UVRect);
            //Rects = MeshAnalyzer.FilterRects(Rects, Mesh.uv);
        }

        // TODO: Refactor this function (it is confusing)
        public void GenerateMateriaSlots()
        {
            var materiaSlotCount = 0;

            // Rebuild Materia Slots
            if (MateriaSetterSlots != null)
            {
                var newMateriaSlots = new List<MateriaSlot>();

                foreach (var slot in MateriaSetterSlots)
                {
                }

                materiaSlotCount = MateriaSetterSlots.Count;

            }
        }

        public void LoadAtlas(MateriaAtlas atlas)
        {
            Renderer.sharedMaterial = atlas.Material;

            for (int i = 0; i < MateriaSetterSlots.Count; i++)
            {
                var slot = MateriaSetterSlots[i];

                if (atlas.AtlasItems.ContainsKey(slot.Tag))
                {
                    slot.Materia = atlas.AtlasItems[slot.Tag].MateriaSlot.Materia;

                    Debug.Log("MS rect: " + slot.Rect);
                    Debug.Log("Atlas rect: " + atlas.AtlasItems[slot.Tag].Rect);

                    var atlasRect = atlas.AtlasItems[slot.Tag].Rect;
                    if (slot.Rect != atlasRect)
                    {
                        Debug.Log("UVs need to be moved!");
                        ShiftUVs(slot, atlasRect);

                        slot.Rect.Set(atlasRect.x, atlasRect.y, atlasRect.width, atlasRect.height);
                        Debug.Log("MS rect AFTER: " + slot.Rect);
                    }
                }
            }
        }

        private void ShiftUVs(MateriaSetterSlot mss, Rect atlasRect)
        {
            var uvs = new Vector2[Mesh.uv.Length];
            Mesh.uv.CopyTo(uvs, 0);

            foreach (var item in mss.UVs)
            {
                var index = item.Key;
                var uv = item.Value;

                var widthMultiplier = atlasRect.width / mss.Rect.width; // 0.5
                var heightMultiplier = atlasRect.height / mss.Rect.height; // 1.0
                var xShift = atlasRect.x - mss.Rect.x;
                var yShift = atlasRect.y - mss.Rect.y;

                uv = Scale2D(uv, new Vector2(widthMultiplier, heightMultiplier), mss.Rect.center);
                //uv.x *= widthMultiplier;
                //uv.y *= heightMultiplier;

                uv.x += xShift;
                uv.y += yShift;


                uvs[index] = uv;
            }

            Mesh.uv = uvs;
        }

        private Vector2 Scale2D(Vector2 vector, Vector2 scale, Vector2 pivot)
        {
            return new Vector2(pivot.x + scale.x * (vector.x - pivot.x), pivot.y + scale.y * (vector.y - pivot.y));
        }

        private Vector2[] ScaleUVs(Vector2[] uvs, Vector2 scale, Vector2 pivot)
        {
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = Scale2D(uvs[i], scale, pivot);
            }

            return uvs;
        }
    }
}