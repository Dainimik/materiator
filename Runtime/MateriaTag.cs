using System;

namespace Materiator
{
    [Serializable]
    public class MateriaTag
    {
        public string Name;
        public string Description;

        public MateriaTag(string name = null, string description = null)
        {
            Name = name;
            Description = description;
        }
    }
}