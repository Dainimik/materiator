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

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            SetUpMesh();
        }

        public void LoadAtlas(MateriaAtlas atlas)
        {
            if (atlas == null || Renderer == null) return;

            Renderer.sharedMaterial = atlas.Material;

            foreach (var slot in MateriaSetterSlots)
            {
                if (atlas.AtlasItems.ContainsKey(slot.Tag))
                {
                    var destRect = atlas.AtlasItems[slot.Tag].Rect;
                    if (slot.Rect != destRect)
                    {
                        MeshUtils.ShiftUVs(slot.MeshData, destRect);
                        slot.Rect.Set(destRect.x, destRect.y, destRect.width, destRect.height);
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

        private void SetUpMesh()
        {
            Renderer = GetComponent<Renderer>();

            var mf = GetComponent<MeshFilter>();
            var smr = GetComponent<SkinnedMeshRenderer>();

            if (smr != null)
            {
                if (MateriaSetterSlots.Count != 0)
                {
                    if (smr.sharedMesh != MateriaSetterSlots[0].MeshData.Mesh)
                    {
                        smr.sharedMesh = MateriaSetterSlots[0].MeshData.Mesh;
                    }
                }
            }
            else if (mf != null)
            {
                if (MateriaSetterSlots.Count > 0)
                {
                    if (mf.sharedMesh != MateriaSetterSlots[0].MeshData.Mesh)
                    {
                        mf.sharedMesh = MateriaSetterSlots[0].MeshData.Mesh;
                    }
                }
            }
        }
    }
}