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
        public ShaderData ShaderData;

        [SerializeReference]
        [ShaderProperty(typeof(ShaderProperty))]
        public List<ShaderProperty> Properties = new List<ShaderProperty>();

#if UNITY_EDITOR
        public Texture2D PreviewIcon;

        public void AddProperties(List<ShaderProperty> properties)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];

                ShaderProperty materiaProperty = null;

                if (property.GetType() == typeof(ColorShaderProperty))
                {
                    var colorProp = (ColorShaderProperty)property;
                    materiaProperty = new ColorShaderProperty(colorProp.Name, colorProp.Value);
                }
                else if (property.GetType() == typeof(FloatShaderProperty))
                {
                    var floatProp = (FloatShaderProperty)property;
                    materiaProperty = new FloatShaderProperty(
                        floatProp.Name,
                        new Vector4(floatProp.R, floatProp.G, floatProp.B, floatProp.A),
                        new string[] { floatProp.RChannel, floatProp.GChannel, floatProp.BChannel, floatProp.AChannel }
                        );
                }

                Properties.Add(materiaProperty);
            }
        }
#endif
    }
}