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
        public string SavePath;
        public int GridSize = 4;
        public bool PackAssets;
        public MateriaTags MateriaTags;
        public MaterialData DefaultMaterialData;
        public ShaderData DefaultShaderData;
        public Materia DefaultMateria;
        //public MaterialGlobalIlluminationFlags GlobalIlluminationFlag;
        public Color HighlightColor;
        public HighlightMode HighlightMode;
        public FilterMode FilterMode;

        public string DefaultTag { get { return "-"; } }

        [HideInInspector] public Color GUIGray = new Color(0.75f, 0.75f, 0.75f, 1f);
        [HideInInspector] public Color GUIGreen = new Color(0f, 0.75f, 0f, 1f);
        [HideInInspector] public Color GUIRed = new Color(0.75f, 0f, 0f, 1f);
    }
}

