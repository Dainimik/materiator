using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    public class MateriaSetterData : ScriptableObject
    {
        public List<MateriaSlot> MateriaSlots;
        public MateriaPreset MateriaPreset;
        public ShaderData ShaderData;
        public Material Material;
        public Textures Textures;
    }
}