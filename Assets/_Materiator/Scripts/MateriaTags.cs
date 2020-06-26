using UnityEngine;

namespace Materiator
{
    public class MateriaTags : ScriptableObject
    {
        public SerializableDictionary<int, string> MateriaTagDictionary;
        public SerializableIntMateriaDictionary Materia;
    }
}