using System;

namespace Materiator
{
    [Serializable]
    public class MateriaSlot
    {
        public int ID;
        public Materia Materia;
        public string MateriaTag;

        public MateriaSlot(int id)
        {
            ID = id;
            Materia = Utils.Settings.DefaultMateria;
            MateriaTag = "-";
        }
    }
}
