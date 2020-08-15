using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public static class AtlasFactory
    {
        public static void CreateAtlas(KeyValuePair<MaterialData, List<MateriaSetter>> group, string path, MateriaAtlas existingAtlas = null)
        {
            var materiaSetters = group.Value;
            var materiaSetterCount = materiaSetters.Count;

            var prefabs = new List<GameObject>();
            var prefabPaths = new List<string>();

            var rects = CalculateRects(materiaSetterCount, SystemData.Settings.GridSize);
            var rectIndex = 0;
            var atlasGridSize = CalculateAtlasSize(materiaSetterCount, SystemData.Settings.GridSize);

            var includeAllPrefabs = false;

            var dir = AssetUtils.GetDirectoryName(path);
            var atlasName = AssetUtils.GetFileName(path);
            MateriaAtlas atlas = null;

            if (existingAtlas == null || existingAtlas.GridSize.x < atlasGridSize.x || existingAtlas.GridSize.y < atlasGridSize.y)
            {
                //atlas = CreateMateriaAtlasAsset(dir, atlasName, materialData, atlasGridSize);
                includeAllPrefabs = true;
            }
            else
            {
                atlas = existingAtlas;
            }

            foreach (var ms in group.Value)
            {
                if (ms.MateriaSetterData.MateriaAtlas != null && !includeAllPrefabs)
                    continue;

                var root = ms.transform.root;
                var prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(root));
                if (!prefabs.Contains(prefabGO))
                {
                    prefabs.Add(prefabGO);
                    prefabPaths.Add(AssetDatabase.GetAssetPath(root));
                }
            }

            var nullSlotIterator = 0;
            var nullSlotIndices = new List<int>();
            foreach (var kvp in atlas.AtlasItems)
            {
                if (kvp.Value.MateriaSetterData == null)
                    nullSlotIndices.Add(kvp.Key);
            }

            var processedPrefabs = new HashSet<GameObject>();
            var skipSavingPrefab = false;

            try
            {
                AssetDatabase.StartAssetEditing();

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
                                continue;

                            var data = ms[j].MateriaSetterData;

                            var atlasedMesh = Utils.CopyMesh(ms[j].MateriaSetterData.NativeMesh);
                            atlasedMesh.uv = MeshAnalyzer.RemapUVs(atlasedMesh.uv, rects[rectIndex]);

                            var prefabMS = prefabs[i].GetComponentsInChildren<MateriaSetter>().Where(setter => setter.MateriaSetterData == data).FirstOrDefault();
                            if (nullSlotIndices.Count > 0)
                            {
                                atlas.AtlasItems[nullSlotIndices[nullSlotIterator]].MateriaSetter = prefabMS;
                                atlas.AtlasItems[nullSlotIndices[nullSlotIterator]].MateriaSetterData = data;

                                nullSlotIterator++;
                            }
                            else
                            {
                                if (!atlas.AtlasItems.ContainsKey(i))
                                    atlas.AtlasItems.Add(i, new MateriaAtlasItem(prefabMS, data));
                            }

                            atlas.MaterialData = group.Key;
                            atlas.GridSize = atlasGridSize;

                            var rectInt = Utils.GetRectIntFromRect(atlasGridSize, rects[rectIndex]);

                            foreach (var tex in data.Textures.Texs)
                            {
                                var colors = tex.Value.GetPixels32();
                                atlas.Textures.Texs.Where(t => t.Key == tex.Key).FirstOrDefault().Value.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, colors);
                            }

                            if (data.AtlasedMesh != null)
                                AssetDatabase.RemoveObjectFromAsset(data.AtlasedMesh);

                            AssetDatabase.AddObjectToAsset(atlasedMesh, data);

                            data.MateriaAtlas = atlas;
                            data.AtlasedMesh = atlasedMesh;
                            data.AtlasedUVRect = rects[rectIndex];
                            data.AtlasedGridSize = atlasGridSize;

                            ms[j].LoadAtlas(atlas);

                            rectIndex++;
                        }

                        processedPrefabs.Add(prefab);
                    }
                    else
                        skipSavingPrefab = true;

                    if (!skipSavingPrefab)
                    {
                        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPaths[i]); // Saves changes madate in unloaded scene to prefab
                        PrefabUtility.UnloadPrefabContents(prefab);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[Materiator] Atlas creation was interrupted: " + ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            atlas.Textures.Apply();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            foreach (var item in prefabs)
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(item));
        }

        public static Dictionary<Materia, List<MateriaSetter>> CombineSameValues(List<MateriaSetter> materiaSetters)
        {
            var values = new Dictionary<Materia, List<MateriaSetter>>();

            foreach (var setter in materiaSetters)
            {
                foreach (var slot in setter.MateriaSlots)
                {
                    if (!values.ContainsKey(slot.Materia))
                    {
                        values.Add(slot.Materia, new List<MateriaSetter>() { setter });
                    }
                    else
                    {
                        values[slot.Materia].Add(setter);
                    }
                }
            }

            return values;
        }

        private static MateriaAtlas CreateMateriaAtlasAsset(string directory, string name, Material material, Vector2Int size)
        {
            var atlas = AssetUtils.CreateScriptableObjectAsset<MateriaAtlas>(directory, name);
            //atlas.Textures.CreateTextures(size.x, size.y);
            atlas.Textures.SetNames(name);
            atlas.Material = UnityEngine.Object.Instantiate(material);
            atlas.Material.name = name;

            AssetDatabase.AddObjectToAsset(atlas.Material, atlas);
            atlas.Textures.AddTexturesToAsset(atlas);

            AssetDatabase.SaveAssets();

            atlas.Textures.ImportTextureAssets();
            atlas.AtlasItems = new SerializableDictionary<int, MateriaAtlasItem>();

            return atlas;
        }

        #region These two functions are almost identical
        public static Rect[] CalculateRects(int number, Vector2Int gridSize) //16, 4x4
        {
            Rect[] rects = new Rect[number]; // 16
            var size = CalculateAtlasSize(number, gridSize);

            var sizeMultiplierX = gridSize.x / (float)size.x; // 0.25
            var sizeMultiplierY = gridSize.y / (float)size.y;

            for (int i = 0, y = 0; y < size.y / gridSize.y; y++) // 4
            {
                for (int x = 0; x < size.x / gridSize.x; x++, i++) // 4
                {
                    if (i >= number) break; // 16
                    rects[i] = new Rect
                    (
                        x * sizeMultiplierX, //if 10 then 2.5
                        y * sizeMultiplierY,
                        sizeMultiplierX,// 0.25
                        sizeMultiplierY
                    );
                }
            }

            return rects;
        }

        
        /*This function is borrowed for easier comparison and should be deleted
         * public static Rect[] CalculateRects(Vector2Int size, Rect offset) // 4, 0
        {
            var rects = new Rect[size.x * size.y]; // 16
            var rectSize = new Vector2();

            rectSize.x = 1 / (float)size.x * offset.width; // 0.25
            rectSize.y = 1 / (float)size.y * offset.height;

            for (int i = 0, y = 0; y < size.y; y++) // 4
            {
                for (int x = 0; x < size.x; x++, i++) // 4
                {
                    if (i >= size.x * size.y) break; // 16

                    rects[i] = new Rect
                    (
                        offset.x + (x / (float)size.x * offset.width), // if 10 then 2.5
                        offset.y + (y / (float)size.y * offset.height),
                        rectSize.x,
                        rectSize.y
                    );

                    rects[i].xMin = rects[i].x;
                    rects[i].yMin = rects[i].y;
                    rects[i].xMax = rects[i].x + rectSize.x;
                    rects[i].yMax = rects[i].y + rectSize.y;
                }
            }

            return rects;
        }*/
        #endregion

        public static Vector2Int CalculateAtlasSize(int numberOfMeshes, Vector2Int atlasEntrySize)
        {
            var ranges = new Vector2[] { new Vector2(0, 2), new Vector2(1, 5), new Vector2(4, 17), new Vector2(16, 65), new Vector2(64, 257), new Vector2(256, 1025), new Vector2(1024, 4097), new Vector2(4096, 16385), new Vector2(16384, 65537), new Vector2(65536, 262145), new Vector2(262144, 1048577), new Vector2(1048576, 4194305) };
            var size = atlasEntrySize; // Minimum atlas size
            for (int i = 0; i < ranges.Length; i++)
            {
                if (numberOfMeshes > ranges[i].x && numberOfMeshes < ranges[i].y)
                {
                    // This is temporary
                    size.x *= (int)Mathf.Pow(2, i);
                    size.y *= (int)Mathf.Pow(2, i);
                }
            }
            return size;
        }
    }
}