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
        public static MateriatorSettings Instance { get { return _instance; } private set { } }
        private static MateriatorSettings _instance;

        public bool PackAssets;
        public ShaderData DefaultShaderData;
        public Materia DefaultMateria;
        //public MaterialGlobalIlluminationFlags GlobalIlluminationFlag;
        public Color HighlightColor;
        public HighlightMode HighlightMode;
        public FilterMode FilterMode;

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            _instance = Resources.Load<MateriatorSettings>("MateriatorSettings");
        }

        private void OnEnable()
        {
            _instance = Resources.Load<MateriatorSettings>("MateriatorSettings");
        }
    }
}

