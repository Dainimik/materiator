using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Material Data", fileName = "MaterialData")]
    public class MaterialData : ScriptableObject
    {
        public ShaderData ShaderData;
        public Material Material;
    }
}