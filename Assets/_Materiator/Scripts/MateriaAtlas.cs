using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    public class MateriaAtlas : ScriptableObject
    {
        public ShaderData ShaderData;
        public Material Material;
        public Textures Textures = new Textures();
        public List<MateriaSetterData> MateriaSetterDatas;
    }
}