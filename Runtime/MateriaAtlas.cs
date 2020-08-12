using UnityEngine;

namespace Materiator
{
    public class MateriaAtlas : ScriptableObject
    {
        public Vector2Int GridSize;

        public MaterialData MaterialData;
        public Material Material;
        public Textures Textures = new Textures();

        public SerializableDictionary<int, MateriaAtlasItem> AtlasItems;
    }
}