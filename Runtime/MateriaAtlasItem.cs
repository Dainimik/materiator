using System;
using UnityEngine;

namespace Materiator
{
    [Serializable]
    public class MateriaAtlasItem
    {
        public MateriaSlot MateriaSlot;
        public Rect Rect;

        [NonSerialized]
        public Textures Textures;

        public MateriaAtlasItem(MateriaSlot materiaSlot, Textures textures)
        {
            MateriaSlot = materiaSlot;
            Textures = textures;
        }
    }
}