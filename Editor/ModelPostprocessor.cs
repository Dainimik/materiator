using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class ModelPostprocessor : AssetPostprocessor
    {
        // TODO: This function needs to be removed as it is here only due to convenience
        private void OnPostprocessModel(GameObject g)
        {
            var modelImporter = assetImporter as ModelImporter;

            modelImporter.bakeAxisConversion = true;

            modelImporter.SaveAndReimport();
        }

        private void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] names, object[] values)
        {
            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var value = values[i];

                switch (name)
                {
                    case "Materiator":
                        ProcessMateriatorTag(value.ToString(), go);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void ProcessMateriatorTag(string value, GameObject go)
        {
            var materiaTags = AssetUtils.FindAssets<MateriaTag>();
            var info = CreateFromJSON(value.Remove(0, 1));

            var ms = go.AddComponent<MateriaSetter>();
            ms.MateriaSetterSlots = new List<MateriaSetterSlot>();
            ms.OriginalMesh = MeshUtils.GetSharedMesh(go); // I need OriginalMesh set here because it needs to be set without MateriaSetter awake triggering.
            ms.Renderer = go.GetComponent<Renderer>();

            var i = 0;
            foreach (var data in info.Data)
            {
                var name = data.M;
                var rect = GetRectFromFloatArray(data.R);

                var tag = materiaTags.FirstOrDefault(tag => tag.name == name);
                var slot = new MateriaSetterSlot(i, rect, name, tag != null ? tag : null)
                {
                    MeshData = GetMeshData(rect, ms.OriginalMesh)
                };
                
                ms.MateriaSetterSlots.Add(slot);

                i++;
            }

            var defaultAtlas = SystemData.Settings.DefaultAtlas;
            if (ms.MateriaAtlas == null && defaultAtlas != null)
                ms.MateriaAtlas = defaultAtlas;
        }

        private static MeshData GetMeshData(Rect rect, Mesh mesh)
        {
            var meshData = new MeshData();

            var indices = new List<int>();
            var colors = new List<Color>();
            var uvs = new List<Vector2>();

            for (var i = 0; i < mesh.uv.Length; i++)
            {
                var uv = mesh.uv[i];
                var color = Color.white;

                if (mesh.colors.Length > 0)
                    color = mesh.colors[i];

                if (rect.Contains(uv))
                {
                    indices.Add(i);
                    colors.Add(color);
                    uvs.Add(uv);
                }
            }

            meshData.Mesh = mesh;
            meshData.Indices = indices.ToArray();
            meshData.Colors = colors.ToArray();
            meshData.UVs = uvs.ToArray();

            return meshData;
        }

        private static Rect GetRectFromFloatArray(float[] array)
        {
            return array.Length != 4 ? new Rect() : new Rect(array[0], array[1], array[2], array[3]);
        }

        private static MateriatorInfoCollection CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<MateriatorInfoCollection>(jsonString);
        }

        [System.Serializable]
        public struct MateriatorInfoCollection
        {
            [System.Serializable]
            public struct MateriatorInfo
            {
                public string M;
                public float[] R;
            }

            public MateriatorInfo[] Data;
        }
    }
}