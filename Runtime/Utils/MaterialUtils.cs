using System.Linq;
using UnityEngine;

namespace Materiator
{
    public static class MaterialUtils
    {
        /// <summary>
        /// Update the current Renderer's material
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="material">Material to replace the current one with.</param>
        /// <param name="previousMaterial">Current material that will be replaced.</param>
        public static void UpdateRendererMaterials(Renderer renderer, Material material, ref Material previousMaterial)
        {
            if (!renderer.sharedMaterials.Contains(material))
            {
                if (previousMaterial != null && renderer.sharedMaterials.Contains(previousMaterial))
                {
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        if (renderer.sharedMaterials[i] == previousMaterial)
                        {
                            var materials = (Material[])renderer.sharedMaterials.Clone();
                            materials[i] = material;

                            renderer.sharedMaterials = materials;
                        }
                    }
                }
                else
                {
                    if (renderer.sharedMaterials.Length != 0 && GetNumberOfNullMaterials(renderer.sharedMaterials) != 1)
                    {
                        var materials = CloneAndExpandMaterialArray(renderer.sharedMaterials, 1);
                        materials[materials.Length - 1] = material;

                        renderer.sharedMaterials = materials; 
                    }
                    else
                    {
                        renderer.sharedMaterial = material;
                    }
                }

                previousMaterial = material;
            }
        }

        private static int GetNumberOfNullMaterials(Material[] materials)
        {
            var result = 0;

            foreach (var material in materials)
                if (material == null)
                    result++;

            return result;
        }

        private static Material[] CloneAndExpandMaterialArray(Material[] array, int count = 0)
        {
            var newArray = new Material[array.Length + count];

            for (int i = 0; i < array.Length; i++)
                newArray[i] = array[i];

            return newArray;
        }
    }
}