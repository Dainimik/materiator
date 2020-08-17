using System;
using UnityEngine;

namespace Materiator
{
    [Serializable]
    public sealed class ColorShaderProperty : ShaderProperty
    {
        public Color Value;
        public float Multiplier = 1;

        public ColorShaderProperty(string propertyName, Color? value = null) : base(propertyName)
        {
            if (value == null)
                value = Color.black;

            Value = (Color)value;
        }
    }
}