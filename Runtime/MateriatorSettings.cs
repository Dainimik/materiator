#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Materiator
{
    public enum HighlightMode
    {
        WhileLMBHeld,
        Continuous
    }

    public class MateriatorSettings : ScriptableObject
    {
        public Vector2Int GridSize = new Vector2Int(4, 4);
        public bool PackAssets;

        [HideInInspector]
        public MateriaTags MateriaTags;

        [HideInInspector]
        public readonly Rect UVRect = new Rect(0f, 0f, 1f, 1f);

        public MaterialData DefaultMaterialData;
        public ShaderData DefaultShaderData;
        public Materia DefaultMateria;
        public MateriaTag DefaultTag = new MateriaTag("-");

        //public MaterialGlobalIlluminationFlags GlobalIlluminationFlag;
        public Color HighlightColor;
        public HighlightMode HighlightMode;
        public FilterMode FilterMode;

#if UNITY_EDITOR
        [HideInInspector] public readonly Color GUIGray = new Color(0.75f, 0.75f, 0.75f, 1f);
        [HideInInspector] public readonly Color GUIGreen = new Color(0f, 0.75f, 0f, 1f);
        [HideInInspector] public readonly Color GUIRed = new Color(0.75f, 0f, 0f, 1f);
#endif
    }
}

