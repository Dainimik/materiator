using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    [DisallowMultipleComponent]
    public class MateriaSetter : MonoBehaviour
    {
        public Mesh Mesh;
        public Renderer Renderer;
        public MeshFilter MeshFilter;
        public MeshRenderer MeshRenderer;
        public SkinnedMeshRenderer SkinnedMeshRenderer;

        public MateriaAtlas MateriaAtlas;
        public List<MateriaSetterSlot> MateriaSetterSlots;

        public Mesh AdditionalMesh;

        public void Initialize()
        {
            GetMeshReferences();

            if (AdditionalMesh == null)
                AdditionalMesh = MeshUtils.CopyMesh(Mesh);
        }

        public void LoadAtlas(MateriaAtlas atlas)
        {
            if (atlas == null) return;

            Renderer.sharedMaterial = atlas.Material;

            foreach (var slot in MateriaSetterSlots)
            {
                if (atlas.AtlasItems.ContainsKey(slot.Tag))
                {
                    var atlasRect = atlas.AtlasItems[slot.Tag].Rect;
                    if (slot.Rect != atlasRect)
                    {
                        ShiftUVs(slot, atlasRect);

                        slot.Rect.Set(atlasRect.x, atlasRect.y, atlasRect.width, atlasRect.height);
                    }

                    slot.Materia = atlas.AtlasItems[slot.Tag].MateriaSlot.Materia;
                }
            }
        }

        private void ShiftUVs(MateriaSetterSlot slot, Rect atlasRect)
        {
            var uvs = new Vector2[Mesh.uv.Length];
            Mesh.uv.CopyTo(uvs, 0);

            for (int i = 0; i < slot.MeshData.Indices.Length; i++)
            {
                var index = slot.MeshData.Indices[i];
                var uv = slot.MeshData.UVs[i];

                var widthMultiplier = atlasRect.width / slot.Rect.width; // 0.5
                var heightMultiplier = atlasRect.height / slot.Rect.height; // 1.0
                var xShift = atlasRect.x - slot.Rect.x;
                var yShift = atlasRect.y - slot.Rect.y;

                uv = MathUtils.Scale2D(uv, new Vector2(widthMultiplier, heightMultiplier), slot.Rect.center);
                //uv.x *= widthMultiplier;
                //uv.y *= heightMultiplier;

                uv.x += xShift;
                uv.y += yShift;


                uvs[index] = uv;
            }

            //Mesh.uv = uvs;
            AdditionalMesh.uv = uvs;
            MeshRenderer.additionalVertexStreams = AdditionalMesh;
            AdditionalMesh.UploadMeshData(true);

        }

        public void SetVertexColor(MateriaTag tag, Color color, bool replace = false)
        {
            var slot = GetMateriaSetterSlot(tag);

            var colors = new Color[Mesh.colors.Length];
            Mesh.uv.CopyTo(colors, 0);

            for (int i = 0; i < slot.MeshData.Indices.Length; i++)
            {
                var index = slot.MeshData.Indices[i];
                var currentColor = slot.MeshData.Colors[i];

                //currentColor = colors.Length > i ? color * colors[i] : color;
                currentColor = replace ? color : currentColor * color;

                colors[index] = currentColor;
            }

            //Mesh.colors = colors;
            AdditionalMesh.colors = colors;
            MeshRenderer.additionalVertexStreams = AdditionalMesh;
            AdditionalMesh.UploadMeshData(true);
        }

        private MateriaSetterSlot GetMateriaSetterSlot(MateriaTag tag)
        {
            foreach (var item in MateriaSetterSlots)
                if (item.Tag == tag)
                    return item;

            return null;
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
                    Mesh = SkinnedMeshRenderer.sharedMesh;
            }
            else
            {
                Mesh = MeshFilter.sharedMesh;
            }
        }
    }
}