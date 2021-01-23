using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    [DisallowMultipleComponent]
    public class MateriaSetter : MonoBehaviour
    {
        public Renderer Renderer;

        public MateriaAtlas MateriaAtlas;
        public List<MateriaSetterSlot> MateriaSetterSlots;

        public Mesh Mesh;

        private void Awake()
        {
            Initialize();

            SetUpMesh();
            LoadAtlas(MateriaAtlas, Mesh);
        }

        public void Initialize()
        {
            Renderer = GetComponent<Renderer>();
        }

        public void LoadAtlas(MateriaAtlas atlas, Mesh mesh)
        {
            if (atlas == null || Renderer == null) return;

            Renderer.sharedMaterial = atlas.Material;

            foreach (var slot in MateriaSetterSlots)
            {
                if (slot.Tag == null) continue;

                if (atlas.AtlasItems.ContainsKey(slot.Tag))
                {
                    var destRect = atlas.AtlasItems[slot.Tag].Rect;

                    /*
                     * Optimization can be made here to only shift the UVs if (slot.Rect != destRect).
                     * However, when implemenmted this way, slot.Rect refers to the MateriaSetterSlot
                     * rect value which is set when loading an atlas and not the rect value of the actual
                     * UVs of the mesh. Threfore, if we load an atlas in the editor, rect values
                     * are set and when we enter play mode, (slot.Rect == destRect) returns true and
                     * the UVs are not shifted.
                     * */

                    MeshUtils.ShiftUVs(mesh, slot.MeshData, destRect);
                    slot.Rect.Set(destRect.x, destRect.y, destRect.width, destRect.height);

                    slot.Materia = atlas.AtlasItems[slot.Tag].MateriaSlot.Materia;
                }
            }

            MateriaAtlas = atlas;
        }

        public void SetVertexColor(MateriaTag tag, Color color, bool replace = false)
        {
            var meshData = GetMateriaSetterSlotFromTag(tag).MeshData;
            MeshUtils.SetVertexColor(Mesh, meshData, color, replace);
        }

        public void SetVertexColor(string slotName, Color color, bool replace = false)
        {
            var meshData = GetMateriaSetterSlotFromName(slotName).MeshData;
            MeshUtils.SetVertexColor(Mesh, meshData, color, replace);
        }

        private void SetUpMesh()
        {
            Mesh = MeshUtils.CopyMesh(Mesh);
            MeshUtils.SetSharedMesh(Mesh, gameObject);
        }

        private MateriaSetterSlot GetMateriaSetterSlotFromTag(MateriaTag tag)
        {
            foreach (var item in MateriaSetterSlots)
                if (item.Tag == tag)
                    return item;

            return null;
        }

        private MateriaSetterSlot GetMateriaSetterSlotFromName(string name)
        {
            foreach (var item in MateriaSetterSlots)
                if (item.Name == name)
                    return item;

            return null;
        }
    }
}