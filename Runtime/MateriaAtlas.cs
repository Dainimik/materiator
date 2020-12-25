using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Materia Atlas", fileName = "MateriaAtlas")]
    public class MateriaAtlas : MateriatorScriptableObject
    {
        public MaterialData MaterialData;
        public List<MateriaSlot> MateriaSlots;

        public List<MateriaAtlasItem> AtlasItems = new List<MateriaAtlasItem>();

        public Material Material;
        public Textures Textures = new Textures();

        public Vector2Int GridSize;

#if UNITY_EDITOR
        public Texture2D PreviewIcon;
#endif
    }
}