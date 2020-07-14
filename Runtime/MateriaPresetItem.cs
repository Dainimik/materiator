using System;

namespace Materiator
{
    [Serializable]
    public class MateriaPresetItem
    {
        public MateriaTag Tag;
        public Materia Materia;

        public MateriaPresetItem(MateriaTag tag, Materia materia = null)
        {
            Tag = tag;
            Materia = materia;
        }
    }
}