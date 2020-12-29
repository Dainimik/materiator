using System;
using System.Collections.Generic;

namespace Materiator
{
    public enum MateriatorShaderPropertyValueChannel
    {
        R, G, B, A
    }

    [Serializable]
    public class MateriatorShaderProperty
    {
        public string Name;
        public string PropertyName;
#if UNITY_EDITOR
        public ShaderPropertyType Type;
#endif
        public List<MateriatorShaderPropertyValue> Values;

        public MateriatorShaderProperty(string name, string propertyName, ShaderPropertyType type, List<MateriatorShaderPropertyValue> values)
        {
            Name = name;
            PropertyName = propertyName;
            Type = type;
            Values = values;
        }
    }

    [Serializable]
    public class MateriatorShaderPropertyValue
    {
        public string Name;
        public string PropertyName;
        public MateriatorShaderPropertyValueChannel Channel;
        public float Value;

        public MateriatorShaderPropertyValue(string name, string propertyName, MateriatorShaderPropertyValueChannel channel, float value)
        {
            Name = name;
            PropertyName = propertyName;
            Channel = channel;
            Value = value;
        }
    }
}