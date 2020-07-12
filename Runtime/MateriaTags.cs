using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    public class MateriaTags : ScriptableObject
    {
        public List<string> MateriaTagsList = new List<string>();
        public string[] MateriaTagsArray
        {
            get { return MateriaTagsList.ToArray(); }
        }
    }
}