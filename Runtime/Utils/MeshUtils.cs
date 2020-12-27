using UnityEngine;

namespace Materiator
{
    public static class MeshUtils
    {
        public static Mesh CopyMesh(Mesh mesh)
        {
            return Mesh.Instantiate(mesh);
        }
    }
}