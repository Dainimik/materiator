using System;
using UnityEngine;

namespace Materiator
{
    public class ShaderPropertyAttribute : PropertyAttribute
    {
        public Type FieldType;

        public ShaderPropertyAttribute(Type fieldType)
        {
            FieldType = fieldType;
        }
    }
}