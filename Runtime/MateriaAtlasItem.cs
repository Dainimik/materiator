using System;

namespace Materiator
{
    [Serializable]
    public class MateriaAtlasItem
    {
        public MateriaSetter MateriaSetter;
        public MateriaSetterData MateriaSetterData;

        public MateriaAtlasItem(MateriaSetter materiaSetter, MateriaSetterData materiaSetterData)
        {
            MateriaSetter = materiaSetter;
            MateriaSetterData = materiaSetterData;
        }
    }
}