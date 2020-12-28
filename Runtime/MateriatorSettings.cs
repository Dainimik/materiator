#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Materiator
{
    public enum MateriaTextureFormat
    {
        RGB24,
        RGBA32,
        RGB9e5Float,
        RGBAHalf
    }

    public class MateriatorSettings : ScriptableObject
    {
        [Header("Materia Setter Settings")]
        public Vector2Int DefaultGridSize = new Vector2Int(1, 1);

        public MaterialData DefaultMaterialData;
        public ShaderData DefaultShaderData;
        public Materia DefaultMateria;

        public bool PackAssets;

        [HideInInspector]
        public MateriaTagCollection MateriaTags;

        [Header("Texture Settings")]
        public TextureFormat TextureFormat = TextureFormat.RGBAHalf;
        public FilterMode FilterMode;

        [HideInInspector]
        public readonly Rect UVRect = new Rect(0f, 0f, 1f, 1f);

#if UNITY_EDITOR
        [HideInInspector] public readonly Color GUIGray = new Color(0.75f, 0.75f, 0.75f, 1f);
        [HideInInspector] public readonly Color GUIGreen = new Color(0f, 0.75f, 0f, 1f);
        [HideInInspector] public readonly Color GUIRed = new Color(0.75f, 0f, 0f, 1f);
#endif
    }
}

