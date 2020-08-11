using System;

namespace Materiator
{
    [Serializable]
    public abstract class ShaderProperty
    {
        public string Name;

        public ShaderProperty(string propertyName)
        {
            Name = propertyName;
        }
    }
}