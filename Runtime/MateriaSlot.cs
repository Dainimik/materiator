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
            if (materia != null)
                Materia = materia;
            else
                Materia = SystemData.Settings.DefaultMateria;
            
            if (tag != null)
                Tag = tag;         
        }
    }
}
