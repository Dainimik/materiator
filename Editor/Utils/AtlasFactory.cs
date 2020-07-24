using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public static class AtlasFactory
    {
        public static bool CheckMateriaSetterCompatibility(MateriaSetter ms)
        {
            if (ms.IsDirty == false
                && ms.MateriaSetterData != null
                && ms.MateriaSetterData.Material != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void CreateAtlas(KeyValuePair<MaterialData, List<MateriaSetter>> group, Material material, string path, MateriaAtlas existingAtlas = null)
        {
            var msCount = 0;
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
                    msCount++;
                }
            }

            /*if (saveAsNewPrefabs)
            {
                for (int i = 0; i < prefabs.Count; i++)
                {
                    var newPath = AssetUtils.GetDirectoryName(prefabPaths[i]) + "/" + AssetUtils.GetFileName(prefabPaths[i]) + newPrefabSuffix + AssetUtils.GetFileExtension(prefabPaths[i], true);
                    AssetDatabase.CopyAsset(prefabPaths[i], newPath);

                    prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
                    prefabPaths[i] = newPath;
                }
            }*/

            var dir = AssetUtils.GetDirectoryName(path);
            var atlasName = AssetUtils.GetFileName(path);

            var rects = Utils.CalculateRects(msCount);
            var rectIndex = 0;
            var gridSize = Utils.CalculateAtlasSize(msCount);

            var includeAllPrefabs = false;

            MateriaAtlas atlas = null;
            if (existingAtlas == null || existingAtlas.GridSize < gridSize)
            {
                atlas = CreateMateriaAtlasAsset(dir, atlasName, material, gridSize);
                includeAllPrefabs = true;
            }
            else
            {
                atlas = existingAtlas;
            }


            var nullSlotIterator = 0;
            var nullSlotIndices = new List<int>();
            foreach (var kvp in atlas.AtlasEntries)
            {
                if (kvp.Value.MateriaSetterData == null)
                {
                    nullSlotIndices.Add(kvp.Key);
                }
            }



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
                        //var nearestPrefabInstanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(ms[j]);
                        //if (processedPrefabs.Contains(nearestPrefabInstanceRoot))
                        //continue;

                        //processedPrefabs.Add(nearestPrefabInstanceRoot);

                        if (ms[j].MateriaSetterData.MateriaAtlas != null && !includeAllPrefabs)
                        {
                            continue;
                        }

                        var data = ms[j].MateriaSetterData;

                        Mesh atlasedMesh;

                        if (data.AtlasedMesh != null)
                        {
                            atlasedMesh = data.AtlasedMesh;
                        }
                        else
                        {
                            atlasedMesh = Utils.CopyMesh(ms[j].MateriaSetterData.NativeMesh);
                        }

                        
                        var remappedUVs = atlasedMesh.uv;

                        for (var k = 0; k < remappedUVs.Length; k++)
                        {
                            var uv = remappedUVs[k];

                            uv.x = rects[rectIndex].x + (uv.x * rects[rectIndex].width);
                            uv.y = rects[rectIndex].y + (uv.y * rects[rectIndex].height);

                            remappedUVs[k] = uv;
                        }

                        atlasedMesh.uv = remappedUVs;

                        //var newMeshData = CreateMeshData(ms[j].Mesh.name, ms[j].Mesh, atlasedMesh, rects[rectIndex], gridSize, atlas);

                        var prefabMS = prefabs[i].GetComponentsInChildren<MateriaSetter>().Where(setter => setter.MateriaSetterData == data).FirstOrDefault();
                        if (nullSlotIndices.Count > 0)
                        {
                            atlas.AtlasEntries[nullSlotIndices[nullSlotIterator]].MateriaSetter = prefabMS;
                            atlas.AtlasEntries[nullSlotIndices[nullSlotIterator]].MateriaSetterData = data;

                            nullSlotIterator++;
                        }
                        else
                        {
                            if (!atlas.AtlasEntries.ContainsKey(i))
                            {
                                atlas.AtlasEntries.Add(i, new MateriaAtlasEntry(prefabMS, data));
                            }
                        }
                        
                        atlas.MaterialData = group.Key;
                        atlas.GridSize = gridSize;

                        var baseCol = data.Textures.Color.GetPixels32();
                        var metallic = data.Textures.MetallicSmoothness.GetPixels32();
                        var emission = data.Textures.Emission.GetPixels32();

                        var rectInt = Utils.GetRectIntFromRect(gridSize, rects[rectIndex]);

                        atlas.Textures.Color.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, baseCol);
                        atlas.Textures.MetallicSmoothness.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, metallic);
                        atlas.Textures.Emission.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, emission);


                        if (data.AtlasedMesh != null)
                        {
                            AssetDatabase.RemoveObjectFromAsset(data.AtlasedMesh);
                        }
                        AssetDatabase.AddObjectToAsset(atlasedMesh, data);

                        data.MateriaAtlas = atlas;
                        data.AtlasedMesh = atlasedMesh;
                        data.AtlasedUVRect = rects[rectIndex];
                        data.AtlasedGridSize = gridSize;

                        AssetDatabase.SaveAssets();


                        ms[j].LoadAtlas(atlas);

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

            atlas.Textures.Apply();

            AssetDatabase.Refresh();

            foreach (var item in prefabs)
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(item));
        }

        private static MateriaAtlas CreateMateriaAtlasAsset(string directory, string name, Material material, int size)
        {
            var atlas = AssetUtils.CreateScriptableObjectAsset<MateriaAtlas>(directory, name);
            atlas.Textures.CreateTextures(size, size);
            atlas.Textures.SetNames(name);
            atlas.Material = Object.Instantiate(material);
            atlas.Material.name = name;

            AssetDatabase.AddObjectToAsset(atlas.Material, atlas);
            atlas.Textures.AddTexturesToAsset(atlas);

            AssetDatabase.SaveAssets();

            atlas.Textures.ImportTextureAssets();
            atlas.AtlasEntries = new SerializableDictionary<int, MateriaAtlasEntry>();

            return atlas;
        }
    }
}