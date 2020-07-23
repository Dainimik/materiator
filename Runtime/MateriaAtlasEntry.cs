using System;

namespace Materiator
{
    [Serializable]
    public class MateriaAtlasEntry
    {
        public MateriaSetter MateriaSetter;
        public MateriaSetterData MateriaSetterData;

        public MateriaAtlasEntry(MateriaSetter materiaSetter, MateriaSetterData materiaSetterData)
        {
            MateriaSetter = materiaSetter;
            MateriaSetterData = materiaSetterData;
        }
    }
}