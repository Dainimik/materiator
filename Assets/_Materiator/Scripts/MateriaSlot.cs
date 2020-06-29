using System;

namespace Materiator
{
    [Serializable]
    public class MateriaSlot
    {
        public int ID;
        public Materia Materia;
        public string Tag;

        public MateriaSlot(int id, Materia materia = null, string tag = null)
        {
            ID = id;

            if (materia != null)
            {
                Materia = materia;
            }
            else
            {
                Materia = Utils.Settings.DefaultMateria;
            }
            
            if (tag != null)
            {
                Tag = tag;
            }
            else
            {
                Tag = Utils.Settings.DefaultTag;
            }
            
        }
    }
}
