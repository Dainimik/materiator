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

        public List<MateriaSetterSlot> MateriaSetterSlots;

        public MateriaAtlas MateriaAtlas;

        public void Initialize()
        {
            Refresh();
        }

        public void Refresh()
        {
            GetMeshReferences();
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

        public void LoadAtlas(MateriaAtlas atlas)
        {
            if (atlas == null) return;

            Renderer.sharedMaterial = atlas.Material;

            for (int i = 0; i < MateriaSetterSlots.Count; i++)
            {
                var slot = MateriaSetterSlots[i];

                if (atlas.AtlasItems.ContainsKey(slot.Tag))
                {
                    slot.Materia = atlas.AtlasItems[slot.Tag].MateriaSlot.Materia;

                    var atlasRect = atlas.AtlasItems[slot.Tag].Rect;
                    if (slot.Rect != atlasRect)
                    {
                        ShiftUVs(slot, atlasRect);

                        slot.Rect.Set(atlasRect.x, atlasRect.y, atlasRect.width, atlasRect.height);
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