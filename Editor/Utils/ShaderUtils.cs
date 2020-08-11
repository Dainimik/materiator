using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Materiator
{
    public class ShaderUtils : MonoBehaviour
    {
        public static string[] GetProperties(Shader shader)
        {
            var props = new string[ShaderUtil.GetPropertyCount(shader)];
            for (int i = 0; i < props.Length; i++)
            {
                props[i] = ShaderUtil.GetPropertyName(shader, i);
            }

            return props;
        }
    }
}