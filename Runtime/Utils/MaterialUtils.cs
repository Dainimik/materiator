using UnityEngine;

namespace Materiator
{
    public static class MaterialUtils
    {
        public static Material CreateMaterial(Shader shader, string name = null)
        {
            var mat = new Material(shader);
            if (name != null)
                mat.name = name;

            //mat.SetColor(SystemData.Settings.DefaultShaderData.BaseColorPropertyName, Color.white);
            //mat.EnableKeyword(SystemData.Settings.DefaultShaderData.MetallicSmoothnessKeywordName);
            return mat;
        }
    }
}