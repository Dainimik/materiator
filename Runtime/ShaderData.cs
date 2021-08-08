using System;
using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Shader Data", fileName = "ShaderData")]
    public class ShaderData : MateriatorScriptableObject
    {
        public Shader Shader;

        public List<MateriatorShaderProperty> MateriatorShaderProperties = new List<MateriatorShaderProperty>();
        public List<string> Keywords = new List<string>();

        public bool IsEditable = true;
    }

    public enum ShaderPropertyType
    {
        Float,
        Vector2,
        Vector3,
        Vector4,
        Color,
        Texture
    }
}
