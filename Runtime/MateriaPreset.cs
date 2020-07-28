using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Materia Preset", fileName = "MateriaPreset")]
    public class MateriaPreset : MateriatorScriptableObject
    {
        public List<MateriaPresetItem> MateriaPresetItemList = new List<MateriaPresetItem>();
    }
}