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
            editor.MateriaSetter.gameObject.AddComponent<MeshFilter>();
        }

        public static void AddMeshRenderer(MateriaSetterEditor editor)
        {
            editor.MateriaSetter.gameObject.AddComponent<MeshRenderer>();
        }

        public static void AddSkinnedMeshRenderer(MateriaSetterEditor editor)
        {
            editor.MateriaSetter.gameObject.AddComponent<SkinnedMeshRenderer>();
        }

        public static void OpenPrefab(MateriaSetterEditor editor)
        {
            AssetDatabase.OpenAsset(editor.MateriaSetter.gameObject);
        }
    }
}