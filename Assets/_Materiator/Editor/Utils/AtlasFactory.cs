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
            if (ms.MateriaSetterData != null
                && ms.MateriaSetterData.Material != null
                && ms.IsDirty == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void CreateAtlas(KeyValuePair<MaterialData, List<MateriaSetter>> group, Material material, string path, bool saveAsNewPrefabs, string newPrefabSuffix)
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

            var rects = Utils.CalculateRects(msCount);
            var rectIndex = 0;
            var gridSize = Utils.CalculateAtlasSize(msCount);

            var atlas = CreateMateriaAtlasAsset(dir, atlasName, material, gridSize);

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

                        //var newMeshData = CreateMeshData(ms[j].Mesh.name, ms[j].Mesh, atlasedMesh, rects[rectIndex], gridSize, atlas);
                        var data = ms[j].MateriaSetterData;

                        var prefabMS = prefabs[i].GetComponentsInChildren<MateriaSetter>().Where(setter => setter.MateriaSetterData == ms[j].MateriaSetterData).FirstOrDefault();
                        atlas.AtlasEntries.Add(prefabMS, data);
                        
                        atlas.MaterialData = group.Key;
                        atlas.GridSize = gridSize;


                        data.MateriaAtlas = atlas;
                        data.NativeMesh = ms[j].Mesh;
                        data.AtlasedMesh = atlasedMesh;
                        data.AtlasedUVRect = rects[rectIndex];
                        data.AtlasedGridSize = gridSize;

                        ms[j].MateriaSetterData = data;

                        ms[j].MateriaAtlas = atlas;
                        ms[j].NativeMesh = ms[j].Mesh;
                        ms[j].AtlasedMesh = atlasedMesh;

                        var baseCol = data.Textures.Color.GetPixels32();
                        var metallic = data.Textures.MetallicSmoothness.GetPixels32();
                        var emission = data.Textures.Emission.GetPixels32();

                        var rectInt = Utils.GetRectIntFromRect(gridSize, rects[rectIndex]);

                        atlas.Textures.Color.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, baseCol);
                        atlas.Textures.MetallicSmoothness.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, metallic);
                        atlas.Textures.Emission.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, emission);

                        //AssetDatabase.AddObjectToAsset(newMeshData, prefabs[i]);
                        AssetDatabase.AddObjectToAsset(atlasedMesh, data);
                        AssetDatabase.SaveAssets();


                        ms[j].LoadAtlas(atlas);
                        //PopulateMateriaAtlas(ms[j], false, atlas, ms[j].MateriaPreset, rects[rectIndex]);
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
            atlas.AtlasEntries = new SerializableDictionary<MateriaSetter, MateriaSetterData>();

            return atlas;
        }

        /*private static MeshData CreateMeshData(string name, Mesh originalMesh, Mesh atlasedMesh, Rect uvRect, int gridSize, MateriaAtlas atlas)
        {
            var md = ScriptableObject.CreateInstance<MeshData>();

            md.name = name;
            md.AtlasedMesh = atlasedMesh;
            md.OriginalMesh = originalMesh;
            md.UVRect = uvRect;
            md.GridSize = gridSize;
            md.Atlas = atlas;

            return md;
        }*/
    }
}