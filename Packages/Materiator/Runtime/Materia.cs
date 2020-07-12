using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Materia", fileName = "Materia")]
    public class Materia : ScriptableObject
    {
        public Color32 BaseColor;
        [Range(0f, 1f)]
        public float Metallic;
        [Range(0f, 1f)]
        public float Smoothness;
        public bool IsEmissive;
        public Color32 EmissionColor = new Color32(0, 0, 0, 255);
    }
}