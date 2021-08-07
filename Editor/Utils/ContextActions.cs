using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class ContextActions
    {
        public static void Retry(MateriaSetterEditor editor)
        {
            editor.Initialize();
        }

        public static void AddMeshFilter(MateriaSetterEditor editor)
        {
            var mf = editor.MateriaSetter.gameObject.AddComponent<MeshFilter>();
            mf.sharedMesh = editor.MateriaSetter.OriginalMesh;
        }

        public static void AddMeshRenderer(MateriaSetterEditor editor)
        {
            editor.MateriaSetter.Renderer = editor.MateriaSetter.gameObject.AddComponent<MeshRenderer>();
        }

        public static void AddSkinnedMeshRenderer(MateriaSetterEditor editor)
        {
            editor.MateriaSetter.Renderer = editor.MateriaSetter.gameObject.AddComponent<SkinnedMeshRenderer>();
        }

        public static void RevertToOriginalMesh(MateriaSetterEditor editor)
        {
            var originalMesh = editor.MateriaSetter.MateriaSetterSlots[0].MeshData.Mesh;
            var mf = editor.MateriaSetter.gameObject.GetComponent<MeshFilter>();
            if (mf != null)
            {
                mf.sharedMesh = originalMesh;
            }
            else
            {
                var smr = editor.MateriaSetter.gameObject.GetComponent<SkinnedMeshRenderer>();
                if (smr != null)
                {
                    smr.sharedMesh = originalMesh;
                }
            }
        }

        public static void OpenPrefab(MateriaSetterEditor editor)
        {
            AssetDatabase.OpenAsset(editor.MateriaSetter.gameObject);
        }
    }
}