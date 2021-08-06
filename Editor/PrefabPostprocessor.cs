using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class PrefabPostprocessor : AssetPostprocessor
    {
        private void OnPostprocessPrefab(GameObject go)
        {
            var materiaSetters = go.GetComponentsInChildren<MateriaSetter>();

            foreach (var materiaSetter in materiaSetters)
            {
                if (!materiaSetter.IsInitialized)
                    materiaSetter.Initialize();

                var mesh = MeshUtils.CopyMesh(materiaSetter.OriginalMesh);

                context.AddObjectToAsset(mesh.GetHashCode().ToString(), mesh);

                materiaSetter.ExecuteAtlas(null, mesh);

                materiaSetter.Mesh = mesh;
                MeshUtils.SetSharedMesh(mesh, materiaSetter.gameObject);
            }
        }
    }
}
