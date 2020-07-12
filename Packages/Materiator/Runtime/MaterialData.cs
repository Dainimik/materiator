using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Material Data", fileName = "MaterialData")]
    public class MaterialData : ScriptableObject
    {
        public ShaderData ShaderData { get { return _shaderData; } }
        public Material Material { get { return _material; } }

        [SerializeField] private ShaderData _shaderData;
        [SerializeField] private Material _material;

        public void Init(ShaderData shaderData, Material material)
        {
            _shaderData = shaderData;
            _material = material;
        }
    }
}