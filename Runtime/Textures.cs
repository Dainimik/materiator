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
        public SerializableDictionary<string, Texture2D> Texs = new SerializableDictionary<string, Texture2D>();

        public int ID;

        public TextureFormat Format;
        public FilterMode FilterMode;

        public Textures(TextureFormat format = TextureFormat.RGBAFloat, FilterMode filterMode = FilterMode.Bilinear)
        {
            Format = format;
            FilterMode = filterMode;
        }
                                      
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

        public void RemoveTextures(List<MateriatorShaderProperty> props, int width, int height)
        {
            foreach (var tex in Texs.ToArray())
                if (!props.Select(prop => prop.PropertyName).ToArray().Contains(tex.Key) || (Size.x != width || Size.y != height))
                    Texs.Remove(tex.Key);
        }

        public void CreateTextures(List<MateriatorShaderProperty> props, int width, int height, bool temporary = false)
        {
            for (int i = 0; i < props.Count; i++)
            {
                var name = props[i].PropertyName;

                if (!Texs.ContainsKey(props[i].PropertyName))
                {
                    Texs.Add(name, CreateTexture2D(width, height, Format, FilterMode, name, temporary));
                }
                else
                {
                    if (Texs[name] == null || (Size.x != width || Size.y != height))
                    {
                        Texs[name] = CreateTexture2D(width, height, Format, FilterMode, name, temporary);
                    }
                }
            }
        }

        public void SetNames(string name = "")
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

        public void UpdateColors(Rect rect, List<MateriatorShaderProperty> shaderProperties)
        {
            var rectInt = MathUtils.GetRectIntFromRect(Size, rect);
            var numberOfColors = rectInt.width * rectInt.height;

            var colors = new Dictionary<Texture2D, Color[]>();

            foreach (var tex in Texs)
            {
                colors.Add(tex.Value, new Color[numberOfColors]);

                for (int i = 0; i < numberOfColors; i++)
                {
                    foreach (var prop in shaderProperties)
                    {
                        if (prop.PropertyName == tex.Key)
                        {
                            foreach (var value in prop.Values)
                            {
                                switch (value.Channel)
                                {
                                    case MateriatorShaderPropertyValueChannel.R:
                                        colors[tex.Value][i].r = value.Value;
                                        break;
                                    case MateriatorShaderPropertyValueChannel.G:
                                        colors[tex.Value][i].g = value.Value;
                                        break;
                                    case MateriatorShaderPropertyValueChannel.B:
                                        colors[tex.Value][i].b = value.Value;
                                        break;
                                    case MateriatorShaderPropertyValueChannel.A:
                                        colors[tex.Value][i].a = value.Value;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }

                tex.Value.SetPixels(rectInt.x, rectInt.y, rectInt.width, rectInt.height, colors[tex.Value]);
            }

            Apply();
        }

        private Texture2D CreateTexture2D(int x, int y, TextureFormat textureFormat, FilterMode filterMode, string name, bool temporary = false, Color ? color = null)
        {
            var tex = new Texture2D(x, y, textureFormat, false);
            tex.filterMode = filterMode;
            tex.name = name;

            if (temporary)
            {
                tex.hideFlags = HideFlags.HideAndDontSave;
            }

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

        public void AddTexturesToAsset(Object objectToAddTo)
        {
            foreach (var tex in Texs)
                AssetDatabase.AddObjectToAsset(tex.Value, objectToAddTo);
        }

        public void RemoveTexturesFromAsset(bool saveDatabaseChanges = false)
        {
            foreach (var tex in Texs.ToArray())
                AssetDatabase.RemoveObjectFromAsset(tex.Value);

            Texs.Clear();

            if (saveDatabaseChanges)
                AssetDatabase.SaveAssets();
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
                WriteTextureToDisk(tex.Value, dirName + tex.Value.name + ".png", FilterMode);

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