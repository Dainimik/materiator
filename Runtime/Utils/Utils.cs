using UnityEngine;

namespace Materiator
{
    public static class Utils
    {
        public static Material CreateMaterial(Shader shader, string name = null)
        {
            var mat = new Material(shader);
            if (name != null)
                mat.name = name;

            //mat.SetColor(SystemData.Settings.DefaultShaderData.BaseColorPropertyName, Color.white);
            //mat.EnableKeyword(SystemData.Settings.DefaultShaderData.MetallicSmoothnessKeywordName);
            return mat;
        }

        public static void PackTextures(Texture2D[] textures, Rect[] rects, Texture2D atlasTexture)
        {
            var rt = new RenderTexture(atlasTexture.width, atlasTexture.height, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.black);
            for (int i = 0; i < textures.Length; i++)
            {
                Graphics.Blit(textures[i], rt, new Vector2(rects[i].width, rects[i].height), new Vector2(rects[i].x, rects[i].y));
            }
            
            atlasTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            RenderTexture.active = null;
            atlasTexture.Apply(false);
        }

        /// <summary>
        /// Returns absolute rect values (in pixels)
        /// </summary>
        /// <param name="gridSize"></param>
        /// <param name="rect">Rect with relative values (in percent).</param>
        /// <returns></returns>
        public static RectInt GetRectIntFromRect(Vector2Int gridSize, Rect rect)
        {
            return new RectInt((int)(gridSize.x * rect.x), (int)(gridSize.y * rect.y), (int)(gridSize.x * rect.width), (int)(gridSize.y * rect.height));
        }

        public static Mesh CopyMesh(Mesh mesh)
        {
            return Mesh.Instantiate(mesh);
        }

        public static void ShallowCopyFields<P, C>(P source, C destination) where P : class where C : class
        {
            var sourceFields = source.GetType().GetFields();
            var destinationFields = destination.GetType().GetFields();

            foreach (var sourceField in sourceFields)
            {
                foreach (var destinationField in destinationFields)
                {
                    if (sourceField.Name == destinationField.Name && sourceField.FieldType == destinationField.FieldType)
                    {
                        destinationField.SetValue(destination, sourceField.GetValue(source));
                        break;
                    }
                }
            }
        }

        public static Color32[] ColorToColor32Array(Color[] colors)
        {
            Color32[] color32 = new Color32[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                color32[i] = new Color32((byte)(colors[i].r * 255), (byte)(colors[i].g * 255), (byte)(colors[i].b * 255), (byte)(colors[i].a * 255));
            }

            return color32;
        }
    }
}