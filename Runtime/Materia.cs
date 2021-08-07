using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Materia", fileName = "Materia")]
    public class Materia : MateriatorScriptableObject
    {
        public MaterialData MaterialData;

        [SerializeReference]
        //[ShaderProperty(typeof(MateriatorShaderProperty))]
        public List<MateriatorShaderProperty> Properties = new List<MateriatorShaderProperty>();

#if UNITY_EDITOR
        public Texture2D PreviewIcon;

        public void AddProperties(IEnumerable<MateriatorShaderProperty> properties)
        {
            foreach (var property in properties)
            {
                Properties.Add(property);
            }
        }
#endif
    }
}