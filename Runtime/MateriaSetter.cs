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

        public void Initialize()
        {
            Renderer = GetComponent<Renderer>();
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
                        MeshUtils.ShiftUVs(slot.MeshData, slot.Rect, atlasRect);

                        slot.Rect.Set(atlasRect.x, atlasRect.y, atlasRect.width, atlasRect.height);
                    }

                    slot.Materia = atlas.AtlasItems[slot.Tag].MateriaSlot.Materia;
                }
            }
        }

        public void SetVertexColor(MateriaTag tag, Color color, bool replace = false)
        {
            var meshData = GetMateriaSetterSlotFromTag(tag).MeshData;
            MeshUtils.SetVertexColor(meshData, color, replace);
        }

        public void SetVertexColor(string slotName, Color color, bool replace = false)
        {
            var meshData = GetMateriaSetterSlotFromName(slotName).MeshData;
            MeshUtils.SetVertexColor(meshData, color, replace);
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