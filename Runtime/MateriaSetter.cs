using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    [DisallowMultipleComponent]
    public class MateriaSetter : MonoBehaviour
    {
        public bool IsMeshSetUp;
        
        public Renderer Renderer;

        public MateriaAtlas MateriaAtlas;
        public List<MateriaSetterSlot> MateriaSetterSlots;

        public Mesh OriginalMesh;
        public Mesh Mesh;

        public void ExecuteAtlas(MateriaAtlas atlas = null, Mesh mesh = null)
        {
            if (atlas == null) atlas = MateriaAtlas;
            if (mesh == null) mesh = Mesh;

            ShiftUVs(atlas, mesh);
        }

        public void LoadAtlas(MateriaAtlas atlas, Mesh mesh)
        {
            if (atlas == null)
            {
                Debug.LogError("[MATERIA SETTER] LoadAtlas aborted because atlas is null.");
                return;
            }
            if (Renderer == null)
            {
                Debug.LogError("[MATERIA SETTER] LoadAtlas aborted because Renderer is null.");
                return;
            }
            if (mesh == null)
            {
                Debug.LogError("[MATERIA SETTER] LoadAtlas aborted because mesh is null.");
                return;
            }

            Renderer.sharedMaterial = atlas.Material;

            ExecuteAtlas(atlas, mesh);

            MateriaAtlas = atlas;
        }

        public void LoadAtlas()
        {
            LoadAtlas(MateriaAtlas, Mesh);
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

        private void ShiftUVs(MateriaAtlas atlas, Mesh mesh)
        {
            if (atlas == null) return;

            foreach (var slot in MateriaSetterSlots)
            {
                if (slot.Tag == null) continue;
                if (!atlas.AtlasItems.ContainsKey(slot.Tag)) continue;
                
                var destRect = atlas.AtlasItems[slot.Tag].Rect;

                /*
                     * Optimization can be made here to only shift the UVs if (slot.Rect != destRect).
                     * However, when implemented this way, slot.Rect refers to the MateriaSetterSlot
                     * rect value which is set when loading an atlas and not the rect value of the actual
                     * UVs of the mesh. Therefore, if we load an atlas in the editor, rect values
                     * are set and when we enter play mode, (slot.Rect == destRect) returns true and
                     * the UVs are not shifted.
                     * */

                MeshUtils.ShiftUVs(mesh, slot.MeshData, destRect);
                slot.Rect.Set(destRect.x, destRect.y, destRect.width, destRect.height);

                slot.Materia = atlas.AtlasItems[slot.Tag].MateriaSlot.Materia;
            }
        }

        public void SetUpMesh()
        {
            if (IsMeshSetUp) return;
            
            Mesh = MeshUtils.CopyMesh(OriginalMesh);
            MeshUtils.SetSharedMesh(Mesh, gameObject);

            IsMeshSetUp = true;
        }

        public void SetMesh(Mesh mesh)
        {
            Mesh = mesh;
            MeshUtils.SetSharedMesh(mesh, gameObject);
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