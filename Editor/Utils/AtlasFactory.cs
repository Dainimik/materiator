using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class AtlasFactory
    {
        public static MateriaAtlas GenerateAtlas(MateriaAtlas atlas)
        {
            atlas.AtlasItems.Clear();
            atlas.Textures.RemoveTexturesFromAsset();

            GenerateAtlasItemTextures(atlas);
            GenerateAtlasTextures(atlas);

            atlas.Textures.SetTexturesToMaterial(atlas.Material);
            atlas.Textures.AddTexturesToAsset(atlas);

            // AssetDatabase.AddObjectToAsset(atlas.Material, atlas);

            AssetDatabase.SaveAssets();

            return atlas;
        }

        private static void GenerateAtlasTextures(MateriaAtlas atlas)
        {
            atlas.Textures = InitializeTextures(atlas);

            foreach (var texture in atlas.Textures.Texs.ToArray())
            {
                var newTex = new Texture2D(8192, 8192);
                newTex.filterMode = SystemData.Settings.FilterMode;

                var atlasItemTextures = atlas.AtlasItems.Values.SelectMany(item => item.Textures.Texs.Where(tex => tex.Key == texture.Key)).Select(pairs => pairs.Value).ToArray();
                var rects = newTex.PackTextures(atlasItemTextures, 0, 8192, false);

                for (int i = 0; i < atlasItemTextures.Length; i++)
                {
                    atlas.AtlasItems.Values.Where(item => item.Textures.Texs[atlasItemTextures[i].name] == atlasItemTextures[i]).First().Rect = rects[i];
                }

                atlas.Textures.Texs[texture.Key] = newTex;

                for (int i = 0; i < atlasItemTextures.Length; i++)
                {
                    Object.DestroyImmediate(atlasItemTextures[i]);
                }
            }
        }

        private static void GenerateAtlasItemTextures(MateriaAtlas atlas)
        {
            var rect = new Rect(0f, 0f, 1f, 1f);
            foreach (var materiaSlot in atlas.MateriaSlots)
            {
                var textures = InitializeTextures(atlas);

                textures.UpdateColors(rect, materiaSlot.Materia.Properties);
                atlas.AtlasItems.Add(materiaSlot.Tag, new MateriaAtlasItem(materiaSlot, textures));
            }
        }

        private static Textures InitializeTextures(MateriaAtlas atlas)
        {
            var shaderProps = atlas.MaterialData.ShaderData.MateriatorShaderProperties;
            var textures = new Textures();
            var gridSize = SystemData.Settings.DefaultGridSize;

            textures.RemoveTextures(shaderProps, gridSize.x, gridSize.y);
            textures.CreateTextures(shaderProps, gridSize.x, gridSize.y, true);

            return textures;
        }
    }
}