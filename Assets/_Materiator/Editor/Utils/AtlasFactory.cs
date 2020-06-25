/*using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public static class AtlasFactory
    {
        public static bool CheckMateriaSetterCompatibility(MateriaSetter ms)
        {
            if (ms.MateriaPreset != null
                && ms.MateriaPreset.Material != null
                && ms.IsMateriaSetterDirty == false)
            {
                return true;
            }
            else
                return false;
        }

        public static void CreateAtlas(KeyValuePair<ShaderData, List<MateriaSetter>> group, Material material, string path, bool saveAsNewPrefabs, string newPrefabSuffix)
        {
            var csCount = 0;
            List<GameObject> prefabs = new List<GameObject>();
            List<string> prefabPaths = new List<string>();
            foreach (var ms in group.Value)
            {
                if (CheckMateriaSetterCompatibility(ms))
                {
                    var root = ms.transform.root;
                    var prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(root));
                    if (!prefabs.Contains(prefabGO))
                    {
                        prefabs.Add(prefabGO);
                        prefabPaths.Add(AssetDatabase.GetAssetPath(root));
                    }
                    csCount++;
                }
            }

            if (saveAsNewPrefabs)
            {
                for (int i = 0; i < prefabs.Count; i++)
                {
                    var newPath = AssetUtils.GetDirectoryName(prefabPaths[i]) + "/" + AssetUtils.GetFileName(prefabPaths[i]) + newPrefabSuffix + AssetUtils.GetFileExtension(prefabPaths[i], true);
                    AssetDatabase.CopyAsset(prefabPaths[i], newPath);

                    prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
                    prefabPaths[i] = newPath;
                }
            }

            var dir = AssetUtils.GetDirectoryName(path);
            var atlasName = AssetUtils.GetFileName(path);

            var rects = Utils.CalculateRects(csCount);
            var rectIndex = 0;
            var gridSize = Utils.CalculateAtlasSize(csCount);

            var atlas = CreateColorAtlasAsset(dir, atlasName, material, gridSize);

            var processedPrefabs = new HashSet<GameObject>();
            var skipSavingPrefab = false;

            for (var i = 0; i < prefabs.Count; i++)
            {
                var prefab = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefabs[i]));
                var ms = prefab.GetComponentsInChildren<MateriaSetter>();

                if (!processedPrefabs.Contains(prefab))
                {
                    for (int j = 0; j < ms.Length; j++)
                    {
                        var nearestPrefabInstanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(ms[j]);
                        if (processedPrefabs.Contains(nearestPrefabInstanceRoot))
                            continue;

                        processedPrefabs.Add(nearestPrefabInstanceRoot);

                        var atlasedMesh = Utils.CopyMesh(ms[j].Mesh);
                        var remappedUVs = atlasedMesh.uv;

                        for (var k = 0; k < remappedUVs.Length; k++)
                        {
                            var uv = remappedUVs[k];

                            uv.x = rects[rectIndex].x + (uv.x * rects[rectIndex].width);
                            uv.y = rects[rectIndex].y + (uv.y * rects[rectIndex].height);

                            remappedUVs[k] = uv;
                        }

                        atlasedMesh.uv = remappedUVs;
                        if (ms[j].MeshData == null)
                        {
                            var newMeshData = CreateMeshData(ms[j].Mesh.name, ms[j].Mesh, atlasedMesh, rects[rectIndex], gridSize, atlas);

                            atlas.MeshDatas.Add(newMeshData);
                            atlas.MateriaPresets.Add(ms[j].MateriaPreset);
                            atlas.ShaderData = group.Key;

                            ms[j].MeshData = newMeshData;
                            ms[j].MateriaAtlas = atlas;
                            ms[j].Mesh = newMeshData.AtlasedMesh;
                            ms[j].Textures = atlas.Textures;
                            //--------------
                            // This is wrong and needs to be sorted out
                            //--------------
                            ms[j].Material = atlas.Material;
                            ms[j].Renderer.sharedMaterial = atlas.Material;
                            //------------
                            ms[j].SetTextures();
                            AssetDatabase.AddObjectToAsset(newMeshData, prefabs[i]);
                            AssetDatabase.AddObjectToAsset(atlasedMesh, newMeshData);
                            AssetDatabase.SaveAssets();
                        }

                        ms[j].LoadAtlas(atlas);
                        PopulateColorAtlas(ms[j], false, atlas, ms[j].MateriaPreset, rects[rectIndex]);
                        rectIndex++;
                    }

                    processedPrefabs.Add(prefab);
                }
                else
                    skipSavingPrefab = true;

                if (!skipSavingPrefab)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefab, prefabPaths[i]);
                    PrefabUtility.UnloadPrefabContents(prefab);
                }
            }
            AssetDatabase.Refresh();

            foreach (var item in prefabs)
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(item));
        }

        private static MateriaAtlas CreateColorAtlasAsset(string directory, string name, Material material, int size)
        {
            var atlas = AssetUtils.CreateScriptableObjectAsset<MateriaAtlas>(directory, name);

            atlas.Textures.CreateTextures(size, size);
            atlas.Textures.SetNames(name);
            atlas.Material = Object.Instantiate(material);

            AssetDatabase.AddObjectToAsset(atlas.Material, atlas);
            atlas.Textures.AddTexturesToAsset(atlas);

            AssetDatabase.SaveAssets();

            atlas.Textures.ImportTextureAssets();
            atlas.MeshDatas = new List<MeshData>();
            atlas.MateriaPresets = new List<MateriaPreset>();

            return atlas;
        }

        private static void PopulateColorAtlas(MateriaSetter ms, bool copyPixels, MateriaAtlas atlas, MateriaPreset cp, Rect r)
        {
            if (copyPixels)
            {
                // not yet implemented
            }
            else
                ms.Refresh();
        }

        private static MeshData CreateMeshData(string name, Mesh originalMesh, Mesh atlasedMesh, Rect uvRect, int gridSize, MateriaAtlas atlas)
        {
            var md = ScriptableObject.CreateInstance<MeshData>();

            md.name = name;
            md.AtlasedMesh = atlasedMesh;
            md.OriginalMesh = originalMesh;
            md.UVRect = uvRect;
            md.GridSize = gridSize;
            md.Atlas = atlas;

            return md;
        }
    }
}*/