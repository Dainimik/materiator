#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif
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

        public void AddProperties(List<MateriatorShaderProperty> properties)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];

                Properties.Add(property);
            }
        }
#endif
    }
}