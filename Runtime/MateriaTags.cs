using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Materiator
{
    public class MateriaTags : ScriptableObject
    {
        public List<MateriaTag> MateriaTagsList = new List<MateriaTag>();
        public string[] MateriaTagNamesArray
        {
            get { return MateriaTagsList.Select(t => t.Name).ToArray(); }
        }
    }
}