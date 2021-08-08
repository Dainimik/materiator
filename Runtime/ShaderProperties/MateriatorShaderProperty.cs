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
        public ShaderPropertyType Type;
        public List<MateriatorShaderPropertyValue> Values;

        public MateriatorShaderProperty(string name, string propertyName, ShaderPropertyType type = ShaderPropertyType.Vector4, List<MateriatorShaderPropertyValue> values = null)
        {
            Name = name;
            PropertyName = propertyName;
            Type = type;
            Values = values ?? new List<MateriatorShaderPropertyValue>();
        }
    }

    [Serializable]
    public class MateriatorShaderPropertyValue
    {
        public string Name;
        public string PropertyName;
        public MateriatorShaderPropertyValueChannel Channel;
        public float Value;

        public MateriatorShaderPropertyValue(string name = "", string propertyName = "", MateriatorShaderPropertyValueChannel channel = MateriatorShaderPropertyValueChannel.R, float value = 0.0f)
        {
            Name = name;
            PropertyName = propertyName;
            Channel = channel;
            Value = value;
        }
    }
}