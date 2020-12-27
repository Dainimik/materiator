using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Materia Tag", fileName = "MateriaTag")]
    public class MateriaTag: MateriatorScriptableObject
    {
        public string Name;
        public string Description;
    }
}