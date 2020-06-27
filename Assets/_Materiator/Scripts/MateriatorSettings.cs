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
        public ShaderData DefaultShaderData;
        public Materia DefaultMateria;
        //public MaterialGlobalIlluminationFlags GlobalIlluminationFlag;
        public Color HighlightColor;
        public HighlightMode HighlightMode;
        public FilterMode FilterMode;
    }
}

