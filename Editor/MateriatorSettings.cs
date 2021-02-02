using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public enum MateriaTextureFormat
    {
        RGB24,
        RGBA32,
        RGBAHalf,
        RGBAFloat
    }

    public class MateriatorSettings : ScriptableObject
    {
        [Header("Materia Setter Settings")]
        public Vector2Int DefaultGridSize = new Vector2Int(1, 1);

        public MaterialData DefaultMaterialData;
        public ShaderData DefaultShaderData;
        public Materia DefaultMateria;
        public MateriaAtlas DefaultAtlas;

        public bool PackAssets;

        [HideInInspector]
        public readonly Rect UVRect = new Rect(0f, 0f, 1f, 1f);

#if UNITY_EDITOR
        [HideInInspector] public readonly Color GUIGray = new Color(0.75f, 0.75f, 0.75f, 1f);
        [HideInInspector] public readonly Color GUIGreen = new Color(0f, 0.75f, 0f, 1f);
        [HideInInspector] public readonly Color GUIRed = new Color(0.75f, 0f, 0f, 1f);
#endif
    }
}

