using System;
using UnityEngine;

namespace Materiator
{
    [Serializable]
    public abstract class ShaderProperty
    {
        public string Name;
        //[HideInInspector]
        public string PropertyName;

        public ShaderProperty(string name, string propertyName)
        {
            Name = name;
            PropertyName = propertyName;
        }
    }
}