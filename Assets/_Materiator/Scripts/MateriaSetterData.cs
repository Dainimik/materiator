using System;
using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    public class MateriaSetterData : ScriptableObject
    {
        public List<MateriaSlot> MateriaSlots;
        public MateriaPreset MateriaPreset;
        public ShaderData ShaderData;
        public Material Material;
        public Textures Textures;

        public MateriaAtlas MateriaAtlas;
        public Mesh OriginalMesh;
        public Mesh AtlasedMesh;
        public int AtlasedGridSize;
        public int OriginalGridSize;
        public int GridSize;
        public Rect AtlasedUVRect;
    }
}