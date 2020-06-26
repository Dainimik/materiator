using UnityEngine;

namespace Materiator
{
    public class MateriaSetter : MonoBehaviour
    {
        public bool IsInitialized = false;

        public Mesh Mesh;
        public MeshFilter MeshFilter;
        public Renderer Renderer;
        public MeshRenderer MeshRenderer;
        public SkinnedMeshRenderer SkinnedMeshRenderer;

        public SerializableIntMateriaDictionary Materia;
        public SerializableDictionary<int, Rect> FilteredRects;

        public ShaderData ShaderData;

        public void Initialize()
        {
            GetMeshReferences();
            IsInitialized = true;
        }

        public void GetMeshReferences()
        {
            Renderer = GetComponent<Renderer>();
            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();
            SkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

            if (MeshFilter == null)
            {
                if (SkinnedMeshRenderer != null)
                {
                    Mesh = SkinnedMeshRenderer.sharedMesh;
                }
            }
            else
            {
                Mesh = MeshFilter.sharedMesh;
            }
        }
    }
}