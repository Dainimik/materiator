using System;

namespace Materiator
{
    [Serializable]
    public class MateriaSlot
    {
        public int ID;
        public Materia Materia;
        public int MateriaTag;

        public MateriaSlot(int id)
        {
            ID = id;
            Materia = Utils.Settings.DefaultMateria;
            MateriaTag = 0;
        }
    }
}
