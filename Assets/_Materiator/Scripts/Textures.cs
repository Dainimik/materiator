#if UNITY_EDITOR
using Object = UnityEngine.Object;
using System.IO;
using UnityEditor;
#endif
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Materiator
{
    [Serializable]
    public class Textures
    {
        public Texture2D Color;
        public Texture2D MetallicSmoothness;
        public Texture2D Emission;

        public FilterMode FilterMode { get; private set; }

        public string[] Names
        {
            get
            {
                return new string[]
                {
                    Color.name,
                    MetallicSmoothness.name,
                    Emission.name
                };
            }

            set
            {
                Color.name = value[0];
                MetallicSmoothness.name = value[1];
                Emission.name = value[2];
            }
        }

        public Vector2Int Size
        {
            get
            {
                return new Vector2Int(Color.width, Color.height);
            }
        }

        public void CreateTextures(int width, int height)
        {
            FilterMode = Utils.Settings.FilterMode;

            Color = CreateTexture2D(width, height, TextureFormat.RGBA32, FilterMode);
            MetallicSmoothness = CreateTexture2D(width, height, TextureFormat.RGBA32, FilterMode);
            Emission = CreateTexture2D(width, height, TextureFormat.RGBA32, FilterMode, UnityEngine.Color.black);
        }

        public void SetNames(string name)
        {
            Color.name = name + "_Color";
            MetallicSmoothness.name = name + "_MetallicSmoothness";
            Emission.name = name + "_Emission";
        }

        public void Apply()
        {
            Color.Apply();
            MetallicSmoothness.Apply();
            Emission.Apply();
        }

        public void SetTextures(Material material, ShaderData shaderData)
        {
            if (material == null || shaderData == null) return;

            material.SetTexture(shaderData.MainTexturePropertyName, Color);
            material.SetTexture(shaderData.MetallicSmoothnessTexturePropertyName, MetallicSmoothness);
            material.SetTexture(shaderData.SpecularGlossTexturePropertyName, MetallicSmoothness);
            material.SetTexture(shaderData.EmissionTexturePropertyName, Emission);
        }

        public void UpdateColors(SerializableDictionary<int, Rect> rects, List<MateriaSlot> materiaSlots)
        {
            foreach (var rect in rects)
            {
                var rectInt = Utils.GetRectIntFromRect(Utils.Settings.GridSize, rect.Value);
                var numberOfColors = rectInt.width * rectInt.height;

                var baseColors = new Color32[numberOfColors];
                var metallic = new Color32[numberOfColors];
                var emissionColors = new Color32[numberOfColors];

                for (int i = 0; i < numberOfColors; i++)
                {
                    var metallic32 = (byte)(materiaSlots.Where(ms => ms.ID == rect.Key).First().Materia.Metallic * 255);

                    baseColors[i] = materiaSlots.Where(ms => ms.ID == rect.Key).First().Materia.BaseColor;
                    metallic[i] = new Color32(metallic32, metallic32, metallic32, metallic32);
                    emissionColors[i] = materiaSlots.Where(ms => ms.ID == rect.Key).First().Materia.EmissionColor;
                }

                Color.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, baseColors);
                MetallicSmoothness.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, metallic);
                Emission.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, emissionColors);
            }

            Apply();
        }

        private Texture2D CreateTexture2D(int x, int y, TextureFormat textureFormat, FilterMode filterMode, Color? color = null)
        {
            var tex = new Texture2D(x, y, textureFormat, false);
            tex.filterMode = filterMode;
            if (color != null)
            {
                var colors = new Color[x * y];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = color.GetValueOrDefault(UnityEngine.Color.gray);
                }
                tex.SetPixels(colors);
            }
            return tex;
        }

        private void SetFilterMode(FilterMode filterMode)
        {
            Color.filterMode = filterMode;
            MetallicSmoothness.filterMode = filterMode;
            Emission.filterMode = filterMode;
        }

        private void SetWrapMode(TextureWrapMode wrapMode)
        {
            Color.wrapMode = wrapMode;
            MetallicSmoothness.wrapMode = wrapMode;
            Emission.wrapMode = wrapMode;
        }

#if UNITY_EDITOR

        public void RenameTextureAssets(string name)
        {
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(Color), name + "_Color");
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(MetallicSmoothness), name + "_MetallicSmoothness");
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(Emission), name + "_Emission");
        }

        public Textures CloneTextures(FilterMode filterMode, bool removeCloneFromName = false)
        {
            //Don't create new instance here
            var texs = new Textures
            {
                Color = Object.Instantiate(Color),
                MetallicSmoothness = Object.Instantiate(MetallicSmoothness),
                Emission = Object.Instantiate(Color)
            };
            texs.SetFilterMode(filterMode);
            texs.SetWrapMode(TextureWrapMode.Clamp);

            if (removeCloneFromName)
                texs.Names = Names;

            return texs;
        }

        public void CopySerialized(Textures source)
        {
            EditorUtility.CopySerialized(source.Color, Color);
            EditorUtility.CopySerialized(source.MetallicSmoothness, MetallicSmoothness);
            EditorUtility.CopySerialized(source.Emission, Emission);
        }

        public void AddTexturesToAsset(Object objectToAddTo)
        {
            AssetDatabase.AddObjectToAsset(Color, objectToAddTo);
            AssetDatabase.AddObjectToAsset(MetallicSmoothness, objectToAddTo);
            AssetDatabase.AddObjectToAsset(Emission, objectToAddTo);
        }

        public Textures WriteTexturesToDisk(string dirName)
        {
            Color = WriteTextureToDisk(Color, dirName + Color.name + ".png", Utils.Settings.FilterMode);
            MetallicSmoothness = WriteTextureToDisk(MetallicSmoothness, dirName + MetallicSmoothness.name + ".png", Utils.Settings.FilterMode);
            Emission = WriteTextureToDisk(Emission, dirName + Emission.name + ".png", Utils.Settings.FilterMode);
            return this;
        }

        private void SetTextureImporterSettings(Texture2D texture, bool isReadable, FilterMode filterMode, bool generateMips)
        {
            if (texture == null) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Default;

                textureImporter.isReadable = isReadable;
                textureImporter.wrapMode = TextureWrapMode.Clamp;
                textureImporter.filterMode = filterMode;
                textureImporter.mipmapEnabled = generateMips;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;

                AssetDatabase.ImportAsset(assetPath);
                AssetDatabase.Refresh();
            }
        }

        public void ImportTextureAssets()
        {
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Color));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(MetallicSmoothness));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(Emission));
        }

        private Texture2D WriteTextureToDisk(Texture2D texture, string path, FilterMode filterMode)
        {
            File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.Refresh();

            texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            SetTextureImporterSettings(texture, true, filterMode, false);

            return texture;
        }
#endif
    }
}