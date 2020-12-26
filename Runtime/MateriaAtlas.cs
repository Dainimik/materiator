using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Materia Atlas", fileName = "MateriaAtlas")]
    public class MateriaAtlas : MateriatorScriptableObject
    {
        public MaterialData MaterialData;
        public Material Material;
        public List<MateriaSlot> MateriaSlots = new List<MateriaSlot>();

        public SerializableDictionary<MateriaTag, MateriaAtlasItem> AtlasItems = new SerializableDictionary<MateriaTag, MateriaAtlasItem>();
        public Textures Textures = new Textures();

#if UNITY_EDITOR
        public Texture2D PreviewIcon;
#endif
    }
}