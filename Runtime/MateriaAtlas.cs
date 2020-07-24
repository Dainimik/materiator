using UnityEngine;

namespace Materiator
{
    public class MateriaAtlas : ScriptableObject
    {
        public MaterialData MaterialData;
        public Material Material;
        public Textures Textures = new Textures();
        public SerializableDictionary<int, MateriaAtlasEntry> AtlasEntries;
        public int GridSize;
    }
}