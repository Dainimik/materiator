#if UNITY_EDITOR
using UnityEditor;
#endif
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

        [SerializeField]
        private bool IsInitialized;

        private void OnEnable()
        {
            if (!IsInitialized) Init();
        }

        public void Init()
        {
            BaseColor = SystemData.Settings.DefaultMateria.BaseColor;
            Metallic = SystemData.Settings.DefaultMateria.Metallic;
            Smoothness = SystemData.Settings.DefaultMateria.Smoothness;
            IsEmissive = SystemData.Settings.DefaultMateria.IsEmissive;
            EmissionColor = SystemData.Settings.DefaultMateria.EmissionColor;

            IsInitialized = true;
        }

#if UNITY_EDITOR
        public Texture2D PreviewIcon;
#endif
    }
}