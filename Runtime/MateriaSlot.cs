using System;

namespace Materiator
{
    [Serializable]
    public class MateriaSlot
    {
        public int ID;
        public Materia Materia;
        public MateriaTag Tag;

        public MateriaSlot(int id, Materia materia = null, MateriaTag tag = null)
        {
            ID = id;

            if (materia != null)
            {
                Materia = materia;
            }
            else
            {
                Materia = SystemData.Settings.DefaultMateria;
            }
            
            if (tag != null)
            {
                Tag = tag;
            }
            else
            {
                Tag = SystemData.Settings.DefaultTag;
            }
            
        }
    }
}
