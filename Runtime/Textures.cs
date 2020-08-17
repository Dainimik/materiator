#if UNITY_EDITOR
using Object = UnityEngine.Object;
using System.IO;
using UnityEditor;
#endif
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

namespace Materiator
{
    [Serializable]
    public class Textures
    {
        public SerializableDictionary<string, Texture2D> Texs = new SerializableDictionary<string, Texture2D>();

        public int ID;

        public FilterMode FilterMode { get; private set; }
                                      
        public string[] Names
        {
            get
            {
                var names = new string[Texs.Count];

                var i = 0;
                foreach (var tex in Texs)
                {
                    names[i] = tex.Value.name;
                    i++;
                }

                return names;
            }
        }

        // This return size of the first texture because only textures of the same size are supported for now
        public Vector2Int Size
        {
            get
            {
                var tex = Texs.FirstOrDefault();
                return new Vector2Int(tex.Value.width, tex.Value.height);
            }
        }

        public void RemoveTextures(List<ShaderProperty> props, int width, int height)
        {
            foreach (var tex in Texs.ToArray())
                if (!props.Select(prop => prop.Name).ToArray().Contains(tex.Key) || (Size.x != width || Size.y != height))
                    Texs.Remove(tex.Key);
        }

        public void CreateTextures(List<ShaderProperty> props, int width, int height)
        {
            FilterMode = SystemData.Settings.FilterMode;

            for (int i = 0; i < props.Count; i++)
            {
                if (props[i].GetType() == typeof(ColorShaderProperty) || props[i].GetType() == typeof(FloatShaderProperty))
                {
                    if (!Texs.ContainsKey(props[i].Name))
                    {
                        Texs.Add(props[i].Name, CreateTexture2D(width, height, SystemData.Settings.TextureFormat, FilterMode));
                    }
                    else
                    {
                        if (Texs[props[i].Name] == null || (Size.x != width || Size.y != height))
                        {
                            Texs[props[i].Name] = CreateTexture2D(width, height, SystemData.Settings.TextureFormat, FilterMode);
                        }
                    }
                }
            }
        }

        public void SetNames(string name)
        {
            foreach (var tex in Texs)
                tex.Value.name = name + tex.Key;
        }

        public void Apply()
        {
            foreach (var tex in Texs)
                tex.Value.Apply();
        }

        public void SetTexturesToMaterial(Material material)
        {
            foreach (var tex in Texs)
                material.SetTexture(tex.Key, tex.Value);
        }

        public void UpdateColors(IDictionary<int, Rect> rects, List<MateriaSlot> materiaSlots)
        {
            foreach (var rect in rects)
            {
                var rectInt = Utils.GetRectIntFromRect(Size, rect.Value);
                var numberOfColors = rectInt.width * rectInt.height;

                var colors = new Dictionary<Texture2D, Color[]>();

                foreach (var tex in Texs)
                {
                    colors.Add(tex.Value, new Color[numberOfColors]);
                    //var data = tex.Value.GetRawTextureData<Color>();

                    for (int i = 0; i < numberOfColors; i++)
                    {
                        foreach (var prop in materiaSlots.Where(ms => ms.ID == rect.Key).First().Materia.Properties)
                        {
                            if (prop.GetType() == typeof(ColorShaderProperty))
                            {
                                var colorProp = (ColorShaderProperty)prop;
                                if (colorProp.Name == tex.Key)
                                {
                                    colors[tex.Value][i] = (Color)colorProp.Value * colorProp.Multiplier;
                                }
                            }
                            else if (prop.GetType() == typeof(FloatShaderProperty))
                            {
                                var floatProp = (FloatShaderProperty)prop;
                                if (floatProp.Name == tex.Key)
                                {
                                    var r = (byte)(floatProp.R * 255);
                                    var g = (byte)(floatProp.G * 255);
                                    var b = (byte)(floatProp.B * 255);
                                    var a = (byte)(floatProp.A * 255);

                                    colors[tex.Value][i].r = floatProp.R;
                                    colors[tex.Value][i].g = floatProp.G;
                                    colors[tex.Value][i].b = floatProp.B;
                                    colors[tex.Value][i].a = floatProp.A;
                                }
                            }
                        }
                    }

                    tex.Value.SetPixels(rectInt.x, rectInt.y, rectInt.width, rectInt.height, colors[tex.Value]);
                }
            }

            Apply();
        }

        private void FillTextureData(Texture2D texture, NativeArray<Color> data)
        {
            var index = 0;
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    //data[index++] = ((x & y) == 0 ? orange : teal);
                }
            }
        }

        // TODO: This function is a modified copy!!! Merge with original!
        public void UpdateColor(List<ShaderProperty> shaderProperties)
        {
            var rectInt = Utils.GetRectIntFromRect(Size, SystemData.Settings.UVRect);
            var numberOfColors = rectInt.width * rectInt.height;

            var colors = new Dictionary<Texture2D, Color32[]>();
            foreach (var tex in Texs)
            {
                colors.Add(tex.Value, new Color32[numberOfColors]);

                for (int i = 0; i < numberOfColors; i++)
                {
                    foreach (var prop in shaderProperties)
                    {
                        if (prop.GetType() == typeof(ColorShaderProperty))
                        {
                            var colorProp = (ColorShaderProperty)prop;
                            if (colorProp.Name == tex.Key)
                            {
                                colors[tex.Value][i] = colorProp.Value;
                            }
                        }
                        else if (prop.GetType() == typeof(FloatShaderProperty))
                        {
                            var floatProp = (FloatShaderProperty)prop;
                            if (floatProp.Name == tex.Key)
                            {
                                var r = (byte)(floatProp.R * 255);
                                var g = (byte)(floatProp.G * 255);
                                var b = (byte)(floatProp.B * 255);
                                var a = (byte)(floatProp.A * 255);

                                colors[tex.Value][i].r = r;
                                colors[tex.Value][i].g = g;
                                colors[tex.Value][i].b = b;
                                colors[tex.Value][i].a = a;
                            }
                        }
                    }
                }

                //tex.Value.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, colors[tex.Value]);
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
                    colors[i] = color.GetValueOrDefault(Color.gray);
                }
                tex.SetPixels(colors);
            }
            return tex;
        }

        private void SetFilterMode(FilterMode filterMode)
        {
            foreach (var tex in Texs)
                tex.Value.filterMode = filterMode;
        }

        private void SetWrapMode(TextureWrapMode wrapMode)
        {
            foreach (var tex in Texs)
                tex.Value.wrapMode = wrapMode;
        }

#if UNITY_EDITOR

        public void CopyPixelColors(Textures source, Vector2Int sourceGridSize, Rect sourceRect, Vector2Int destinationGridSize, Rect destinationRect)
        {
            var sourceRectInt = new RectInt((int)(sourceRect.x * sourceGridSize.x), (int)(sourceRect.y * sourceGridSize.y), (int)(sourceRect.width * sourceGridSize.x), (int)(sourceRect.height * sourceGridSize.y));
            var destinationRectInt = new RectInt((int)(destinationRect.x * destinationGridSize.x), (int)(destinationRect.y * destinationGridSize.y), (int)(destinationRect.width * destinationGridSize.x), (int)(destinationRect.height * destinationGridSize.y));

            foreach (var tex in source.Texs)
            {
                var sourceColors = tex.Value.GetPixels(sourceRectInt.x, sourceRectInt.y, sourceRectInt.width, sourceRectInt.height);
                Texs.Where(t => t.Key == tex.Key).FirstOrDefault().Value.SetPixels(destinationRectInt.x, destinationRectInt.y, destinationRectInt.width, destinationRectInt.height, sourceColors);
            }

            Apply();
        }

        public void AddTexturesToAsset(Object objectToAddTo)
        {
            foreach (var tex in Texs)
                AssetDatabase.AddObjectToAsset(tex.Value, objectToAddTo);
        }

        private Texture2D AddTextureToAsset(Texture2D texture, Object objectToAddTo)
        {
            AssetDatabase.AddObjectToAsset(texture, objectToAddTo);

            var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(objectToAddTo));

            foreach (var item in assets)
            {
                if (item == texture)
                {
                    texture = (Texture2D)item;
                }
            }

            return texture;
        }

        public Textures WriteTexturesToDisk(string dirName)
        {
            foreach (var tex in Texs)
                WriteTextureToDisk(tex.Value, dirName + tex.Value.name + ".png", SystemData.Settings.FilterMode);

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
            foreach (var tex in Texs)
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex.Value));
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