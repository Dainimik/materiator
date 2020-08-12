using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Materiator
{
    public class MateriaTagCollection : ScriptableObject
    {
        public List<MateriaTag> MateriaTags = new List<MateriaTag>();
        public string[] MateriaTagNames
        {
            get { return MateriaTags.Select(t => t.Name).ToArray(); }
        }
    }
}