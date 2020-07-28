﻿using UnityEngine;

namespace Materiator
{
    public static class Utils
    {
        public static Material CreateMaterial(Shader shader, string name = null)
        {
            var mat = new Material(shader);
            if (name != null)
                mat.name = name;

            mat.SetColor(SystemData.Settings.DefaultShaderData.BaseColorPropertyName, Color.white);
            mat.EnableKeyword(SystemData.Settings.DefaultShaderData.MetallicSmoothnessKeywordName);
            return mat;
        }

        public static void UpdateMaterial(Material material, ShaderData shaderData)
        {
            material.SetColor(shaderData.BaseColorPropertyName, Color.white);
            material.EnableKeyword(shaderData.MetallicSmoothnessKeywordName);
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

        public static Rect[] CalculateRects(int number)
        {
            Rect[] rects = new Rect[number];
            var size = CalculateAtlasSize(number);

            var sizeMultiplierX = 4 / (float)size.x;
            var sizeMultiplierY = 4 / (float)size.y;

            for (int i = 0, y = 0; y < size.y / 4; y++)
            {
                for (int x = 0; x < size.x / 4; x++, i++)
                {
                    if (i >= number) break;
                    rects[i] = new Rect(x * sizeMultiplierX, y * sizeMultiplierY, sizeMultiplierX, sizeMultiplierY);
                }
            }

            return rects;
        }

        public static RectInt GetRectIntFromRect(Vector2Int gridSize, Rect rect)
        {
            return new RectInt((int)(gridSize.x * rect.x), (int)(gridSize.y * rect.y), (int)(gridSize.x * rect.width), (int)(gridSize.y * rect.height));
        }

        public static Vector2Int CalculateAtlasSize(int numberOfMeshes)
        {
            var ranges = new Vector2[] { new Vector2(0, 2), new Vector2(1, 5), new Vector2(4, 17), new Vector2(16, 65), new Vector2(64, 257), new Vector2(256, 1025), new Vector2(1024, 4097), new Vector2(4096, 16385), new Vector2(16384, 65537), new Vector2(65536, 262145), new Vector2(262144, 1048577), new Vector2(1048576, 4194305) };
            var size = new Vector2Int(4, 4); // Minimum atlas size is hardcoded here
            for (int i = 0; i < ranges.Length; i++)
            {
                if (numberOfMeshes > ranges[i].x && numberOfMeshes < ranges[i].y)
                {
                    // This is temporary
                    size.x = 4 * (int)Mathf.Pow(2, i);
                    size.y = 4 * (int)Mathf.Pow(2, i);
                }
            }
            return size;
        }

        public static Mesh CopyMesh(Mesh mesh)
        {
            return Mesh.Instantiate(mesh);
        }

        public static Texture2D GenerateThumbnail(Material material, Color backgroundColor)
        {
            RuntimePreviewGenerator.BackgroundColor = backgroundColor;
            RuntimePreviewGenerator.PreviewDirection = new Vector3(-0.50f, 0.80f, 1f);
            RuntimePreviewGenerator.Padding = -0.20f;
            var tex = RuntimePreviewGenerator.GenerateMaterialPreview(material, PrimitiveType.Sphere, 128, 128);
            return tex;
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