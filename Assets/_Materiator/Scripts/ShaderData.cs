using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Shader Data", fileName = "ShaderData")]
    public class ShaderData : ScriptableObject
    {
        public Shader Shader;
        public string BaseColorPropertyName;
        public string MainTexturePropertyName;
        public string MetallicSmoothnessTexturePropertyName;
        public string SpecularGlossTexturePropertyName;
        public string EmissionColorPropertyName;
        public string EmissionTexturePropertyName;
        public string MetallicSmoothnessKeywordName;
        public string EmissionKeywordName;
        public bool IsEditable = true;
    }
}
