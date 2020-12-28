using UnityEngine;

namespace Materiator
{
    public static class MeshUtils
    {
        public static void ShiftUVs(MeshData meshData, Rect sourceRect, Rect destRect)
        {
            var mesh = meshData.Mesh;
            var uvs = new Vector2[mesh.uv.Length];
            mesh.uv.CopyTo(uvs, 0);

            for (int i = 0; i < meshData.Indices.Length; i++)
            {
                var index = meshData.Indices[i];
                var uv = meshData.UVs[i];

                var widthMultiplier = 1f;
                var heightMultiplier = 1f;

                if (sourceRect.width != destRect.width)
                    widthMultiplier = destRect.width;

                if (sourceRect.height != destRect.height)
                    heightMultiplier = destRect.height;

                var xShift = destRect.x - sourceRect.x;
                var yShift = destRect.y - sourceRect.y;

                uv.x = uv.x * widthMultiplier + xShift;
                uv.y = uv.y * heightMultiplier + yShift;

                uvs[index] = uv;
                meshData.UVs[i] = uv;
            }

            mesh.uv = uvs;
        }

        public static void SetVertexColor(MeshData meshData, Color color, bool replace = false)
        {
            var mesh = meshData.Mesh;
            var colors = new Color[mesh.colors.Length];
            mesh.colors.CopyTo(colors, 0);

            for (int i = 0; i < meshData.Indices.Length; i++)
            {
                var index = meshData.Indices[i];
                var currentColor = meshData.Colors[i];

                currentColor = replace ? color : currentColor * color;

                colors[index] = currentColor;
            }

            mesh.colors = colors;
        }

        public static Mesh GetSharedMesh(GameObject go)
        {
            var mf = go.GetComponent<MeshFilter>();

            if (mf == null)
            {
                var smr = go.GetComponent<SkinnedMeshRenderer>();
                if (smr != null)
                    return smr.sharedMesh;
            }
            else
            {
                return mf.sharedMesh;
            }

            return null;
        }

        public static Mesh CopyMesh(Mesh mesh)
        {
            return Mesh.Instantiate(mesh);
        }
    }
}