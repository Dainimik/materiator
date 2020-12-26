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
        public SerializableDictionary<int, Vector2> UVs;
        public MateriaTag Tag;

        public Materia Materia;

        public MateriaSetterSlot(int id, Rect rect, string name = "", MateriaTag tag = null)
        {
            ID = id;
            Name = name;
            Rect = rect;

            if (tag != null)
                Tag = tag;
            else
                Tag = SystemData.Settings.DefaultTag;
        }
    }
}
