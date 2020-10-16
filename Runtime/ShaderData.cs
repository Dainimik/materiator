using System.Collections.Generic;
using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Shader Data", fileName = "ShaderData")]
    public class ShaderData : ScriptableObject
    {
        [HideInInspector] // Temporarily
        public Shader Shader;

        [SerializeReference]
        public List<ShaderProperty> Properties = new List<ShaderProperty>();
        public List<string> Keywords = new List<string>();

        public List<string> AvailableShaderProperties = new List<string>();
        public List<string> SelectedShaderProperties = new List<string>();

        public bool IsEditable = true;
    }
}
