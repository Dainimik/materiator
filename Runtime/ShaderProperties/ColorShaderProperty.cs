using System;
using UnityEngine;

namespace Materiator
{
    [Serializable]
    public sealed class ColorShaderProperty : ShaderProperty
    {
        public Color32 Value;
        public float Multiplier = 1;

        public ColorShaderProperty(string propertyName, Color32? value = null) : base(propertyName)
        {
            if (value == null)
                value = Color.black;

            Value = (Color32)value;
        }
    }
}