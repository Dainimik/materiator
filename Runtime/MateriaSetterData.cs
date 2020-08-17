using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    public class MateriaSetterData : ScriptableObject
    {
        public List<MateriaSlot> MateriaSlots;

        public MateriaPreset MateriaPreset;

        public MaterialData MaterialData;
        public Material Material;
        public Textures Textures;

        public Mesh NativeMesh;
        public Vector2Int NativeGridSize;

        public MateriaAtlas MateriaAtlas;
        public Mesh AtlasedMesh;
        public Rect AtlasedUVRect;
    }
}