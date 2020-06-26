using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Materia Preset", fileName = "MateriaPreset")]
    public class MateriaPreset : ScriptableObject
    {
        public SerializableDictionary<int, string> MateriaCategories;
        public SerializableIntMateriaDictionary Materia;
    }

}