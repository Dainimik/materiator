/*using UnityEngine;

namespace Materiator
{
    public static class Utils
    {
        public static MateriatorSettings Settings = LoadSettings();
        public static Material CreateMaterial(Shader shader, string name = null)
        {
            var mat = new Material(shader);
            if (name != null)
                mat.name = name;

            mat.SetColor(Settings.DefaultShaderData.MainColorPropertyName, Color.white);
            mat.EnableKeyword(Settings.DefaultShaderData.MetallicSmoothnessKeywordName);
            return mat;
        }

        public static void UpdateMaterial(Material material, ShaderData shaderData)
        {
            material.SetColor(shaderData.MainColorPropertyName, Color.white);
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

            var sizeMultiplier = 4 / (float)size;

            for (int i = 0, y = 0; y < size / 4; y++)
            {
                for (int x = 0; x < size / 4; x++, i++)
                {
                    if (i >= number) break;
                    rects[i] = new Rect(x * sizeMultiplier, y * sizeMultiplier, sizeMultiplier, sizeMultiplier);
                }
            }

            return rects;
        }

        public static int CalculateAtlasSize(int numberOfMeshes)
        {
            var ranges = new Vector2[] { new Vector2(0, 2), new Vector2(1, 5), new Vector2(4, 17), new Vector2(16, 65), new Vector2(64, 257), new Vector2(256, 1025), new Vector2(1024, 4097), new Vector2(4096, 16385), new Vector2(16384, 65537), new Vector2(65536, 262145), new Vector2(262144, 1048577), new Vector2(1048576, 4194305) };
            var size = 4;
            for (int i = 0; i < ranges.Length; i++)
            {
                if (numberOfMeshes > ranges[i].x && numberOfMeshes < ranges[i].y)
                {
                    size = 4 * (int)Mathf.Pow(2, i);
                }
            }
            return size;
        }

        public static Mesh CopyMesh(Mesh mesh)
        {
            return Mesh.Instantiate(mesh);
        }

        public static void GenerateColorDataIcons(Materia materia, Material material)
        {
            if (materia.PreviewIcon != null)
                Object.DestroyImmediate(materia.PreviewIcon);

            if (materia.PreviewIconGray != null)
                Object.DestroyImmediate(materia.PreviewIconGray);

            material.SetColor(Settings.DefaultShaderData.MainColorPropertyName, materia.Color);
            material.SetFloat("_Metallic", materia.Metallic);
            material.SetFloat("_Smoothness", materia.Smoothness);
            if (materia.IsEmissive)
            {
                material.EnableKeyword(Settings.DefaultShaderData.EmissionKeywordName);
                material.globalIlluminationFlags = Settings.GlobalIlluminationFlag;
                material.SetColor(Settings.DefaultShaderData.EmissionColorPropertyName, materia.Emission);
            }
            else
            {
                material.DisableKeyword(Settings.DefaultShaderData.EmissionKeywordName);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }

            materia.PreviewIcon = GenerateThumbnail(material, Color.black);
            materia.PreviewIconGray = GenerateThumbnail(material, new Color(0.78431f, 0.78431f, 0.78431f, 1));
        }

        public static Texture2D GenerateThumbnail(Material material, Color backgroundColor)
        {
            RuntimePreviewGenerator.BackgroundColor = backgroundColor;
            RuntimePreviewGenerator.PreviewDirection = new Vector3(-0.50f, 0.80f, 1f);
            RuntimePreviewGenerator.Padding = -0.20f;
            var tex = RuntimePreviewGenerator.GenerateMaterialPreview(material, PrimitiveType.Sphere, 128, 128);
            return tex;
        }

        public static MateriatorSettings LoadSettings()
        {
            return Resources.Load<MateriatorSettings>("MateriatorSettings");
        }
    }
}*/