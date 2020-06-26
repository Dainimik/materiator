using System;

namespace Materiator
{
    [Serializable]
    public class MateriaPresetItem
    {
        public string Tag;
        public Materia Materia;

        public MateriaPresetItem(string tag, Materia materia)
        {
            Tag = tag;
            Materia = materia;
        }
    }
}