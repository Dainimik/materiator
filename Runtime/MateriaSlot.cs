using System;

namespace Materiator
{
    [Serializable]
    public class MateriaSlot
    {
        public Materia Materia;
        public MateriaTag Tag;

        public MateriaSlot(Materia materia = null, MateriaTag tag = null)
        {
            Materia = materia;
            Tag = tag;         
        }
    }
}
