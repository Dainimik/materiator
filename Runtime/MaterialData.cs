using UnityEngine;

namespace Materiator
{
    [CreateAssetMenu(menuName = "Materiator/Material Data", fileName = "MaterialData")]
    public class MaterialData : MateriatorScriptableObject
    {
        public ShaderData ShaderData => _shaderData;
        public Material Material => _material;

        [SerializeField] private ShaderData _shaderData;
        [SerializeField] private Material _material;

        public void Init(ShaderData shaderData, Material material)
        {
            _shaderData = shaderData;
            _material = material;
        }
    }
}