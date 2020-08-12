using System;

namespace Materiator
{
    [Serializable]
    public abstract class ShaderProperty
    {
        public string Name;

        public ShaderProperty(string name)
        {
            Name = name;
        }
    }
}