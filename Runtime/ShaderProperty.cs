using System;
using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
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