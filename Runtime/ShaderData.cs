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
        Color
    }

    [Serializable]
    public class ShaderProperty
    {
        public string FullString;
        public string PropertyName;
        public string Name;
        public ShaderPropertyType Type;
        public List<string> Value;
        public Vector2 Range;

        public ShaderProperty(string fullString, string propertyName, string name, ShaderPropertyType type, List<string> value, Vector2 range = default)
        {
            FullString = fullString;
            PropertyName = propertyName;
            Name = name;
            Type = type;
            Value = value;
            Range = range;
        }
    }
}
