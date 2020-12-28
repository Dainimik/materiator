using System;
using UnityEngine;

namespace Materiator
{
    [Serializable]
    public class MateriaSetterSlot
    {
        public int ID;
        public string Name;
        public Rect Rect;
        public MateriaTag Tag;
        public Materia Materia;
        public MeshData MeshData;

        public MateriaSetterSlot(int id, Rect rect, string name = "", MateriaTag tag = null)
        {
            ID = id;
            Name = name;
            Rect = rect;

            if (tag != null)
                Tag = tag;
        }
    }

    [Serializable]
    public struct MeshData
    {
        public Mesh Mesh;
        public int[] Indices;
        public Color[] Colors;
        public Vector2[] UVs;
    }
}
