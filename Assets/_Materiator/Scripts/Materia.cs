using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Materia", fileName = "Materia")]
    public class Materia : ScriptableObject
    {
        public Color32 BaseColor;
        public float Metallic;
        public float Smoothness;
        public Color EmissionColor;
        public bool IsEmissive;
    }
}