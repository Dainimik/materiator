using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace Materiator
{
    public class PrefabPostprocessor : AssetPostprocessor
    {
        /*
         * If MateriaSetter is removed from an object and the prefab is overriden, the prefab
         * loses the mesh subasset that was associated with the respective MateriaSetter instance
         * that was removed. Possible solutions are to 1) never remove MateriaSetter component, or 2) bake
         * meshes into separate assets on disk.
         */
        private void OnPostprocessPrefab(GameObject go)
        {
            var materiaSetters = go.GetComponentsInChildren<MateriaSetter>();

            foreach (var materiaSetter in materiaSetters)
            {
                Debug.Log("POSTPROCESSING MS: " + materiaSetter);
                //Debug.Log("PREFAB POSTPROCESS MESH SUBASSET: " + CustomPrefabEnvironment.GetMeshSubassetFromAsset(subassets, materiaSetter));
                //if (CustomPrefabEnvironment.GetMeshSubassetFromAsset(subassets, materiaSetter)) continue;
                
                Debug.Log("MS INIT POST: " + materiaSetter.GetComponent<MeshFilter>().sharedMesh);

                Debug.Log("MS INIT POST: " + materiaSetter.GetComponent<MeshFilter>().sharedMesh);
                
                Debug.Log("MS ORG MESH: " + materiaSetter.OriginalMesh);

                var mesh = MeshUtils.CopyMesh(materiaSetter.OriginalMesh);
                
                context.AddObjectToAsset(GetIdentifier(mesh, materiaSetter), mesh);

                materiaSetter.ExecuteAtlas(null, mesh);
                materiaSetter.SetMesh(mesh);

                EditorUtility.SetDirty(materiaSetter);
            }
        }

        private static string GetIdentifier(Mesh mesh, MateriaSetter materiaSetter)
        {
            var identifier = mesh.name;
            foreach (var slot in materiaSetter.MateriaSetterSlots)
            {
                identifier += slot.Name;
            }

            return identifier;
        }
    }
    
 
    // TODO: Move this class to separate file
    [InitializeOnLoad]
    class CustomPrefabEnvironment
    {
        static CustomPrefabEnvironment()
        {
            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
        }
 
        static void OnPrefabStageOpened(PrefabStage prefabStage)
        {
            Debug.Log("OnPrefabStageOpened " + prefabStage.assetPath);
            
            var subassets = AssetDatabase.LoadAllAssetRepresentationsAtPath(prefabStage.assetPath);

            foreach (var subasset in subassets)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(subasset, out var guid, out long localid);
                Debug.Log("MESH: " + subasset + " GUID: " + guid + "LOCAL ID: " + localid);
            }

            foreach (var materiaSetter in prefabStage.prefabContentsRoot.GetComponentsInChildren<MateriaSetter>())
            {
                if (materiaSetter.Mesh != null && MeshUtils.GetSharedMesh(materiaSetter.gameObject) != null) continue;
                
                var mesh = GetMeshSubassetFromAsset(subassets, materiaSetter);

                materiaSetter.SetMesh(mesh);
            }
        }

        public static Mesh GetMeshSubassetFromAsset(UnityEngine.Object[] subassets, MateriaSetter materiaSetter)
        {
            return subassets.FirstOrDefault(a => a.name == materiaSetter.OriginalMesh.name + "(Clone)") as Mesh;
        }
    }
}
