using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class AtlasFactory
    {
        private static MateriaAtlas _atlas;

        private static MateriaSetterData _msData;

        private static Rect[] _rects;
        private static Vector2Int _atlasGridSize;
        private static int _rectIndex;
        private static Mesh _atlasedMesh;

        private static int _nullSlotIterator;
        private static List<int> _nullSlotIndices;

        public static void CreateAtlas(KeyValuePair<MaterialData, List<MateriaSetter>> group, string path, MateriaAtlas existingAtlas = null)
        {
            var materialData = group.Key;
            var materiaSetters = group.Value;
            var materiaSetterCount = materiaSetters.Count;

            _rects = CalculateRects(materiaSetterCount, SystemData.Settings.GridSize);
            _atlasGridSize = CalculateAtlasSize(materiaSetterCount, SystemData.Settings.GridSize);

            _atlas = existingAtlas;
            if (existingAtlas == null)
                _atlas = CreateMateriaAtlasAsset(AssetUtils.GetDirectoryName(path), AssetUtils.GetFileName(path), materialData, _atlasGridSize);

            _nullSlotIterator = 0;
            _nullSlotIndices = GetAtlasNullSlotIndices();

            var processedPrefabs = new HashSet<GameObject>();
            _rectIndex = 0;

            var includeAllPrefabs = DeterminePrefabProcessing(existingAtlas);
            var prefabs = GetPrefabs(materiaSetters, includeAllPrefabs);

            try
            {
                AssetDatabase.StartAssetEditing();

                for (var i = 0; i < prefabs.Length; i++)
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

                            _msData = ms[j].MateriaSetterData;

                            FillAtlasWithItems(prefabs, i);

                            SetAtlasTextureValues();

                            CreateAtlasedMesh();

                            UpdateMateriaSetterData();

                            ms[j].LoadAtlas(_atlas);

                            _rectIndex++;
                        }

                        processedPrefabs.Add(prefab);

                        PrefabUtility.SaveAsPrefabAsset(prefab, AssetDatabase.GetAssetPath(prefabs[i])); // Saves changes madate in unloaded scene to prefab
                    }

                    PrefabUtility.UnloadPrefabContents(prefab);
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

            _atlas.Textures.Apply();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            foreach (var item in prefabs)
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(item));
        }

        private static bool DeterminePrefabProcessing(MateriaAtlas atlas)
        {
            if (atlas == null || atlas.GridSize.x < _atlasGridSize.x || atlas.GridSize.y < _atlasGridSize.y)
                return true;
            else
                return false;
        }

        private static GameObject[] GetPrefabs(List<MateriaSetter> setters, bool includeAllPrefabs)
        {
            var prefabs = new List<GameObject>();

            foreach (var ms in setters)
            {
                if (ms.MateriaSetterData.MateriaAtlas != null && !includeAllPrefabs)
                    continue;

                var root = ms.transform.root;
                var prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(root));

                if (!prefabs.Contains(prefabGO))
                    prefabs.Add(prefabGO);
            }

            return prefabs.ToArray();
        }

        private static List<int> GetAtlasNullSlotIndices()
        {
            var indices = new List<int>();

            if (_atlas != null)
                foreach (var kvp in _atlas.AtlasItems)
                    if (kvp.Value.MateriaSetterData == null)
                        indices.Add(kvp.Key);

            return indices;
        }

        private static void FillAtlasWithItems(GameObject[] prefabs, int i)
        {
            var prefabMS = prefabs[i].GetComponentsInChildren<MateriaSetter>().Where(setter => setter.MateriaSetterData == _msData).FirstOrDefault();

            if (_nullSlotIndices.Count > 0)
            {
                _atlas.AtlasItems[_nullSlotIndices[_nullSlotIterator]].MateriaSetter = prefabMS;
                _atlas.AtlasItems[_nullSlotIndices[_nullSlotIterator]].MateriaSetterData = _msData;

                _nullSlotIterator++;
            }
            else
            {
                if (!_atlas.AtlasItems.ContainsKey(i))
                    _atlas.AtlasItems.Add(i, new MateriaAtlasItem(prefabMS, _msData));
            }
        }

        private static void SetAtlasTextureValues()
        {
            var rectInt = Utils.GetRectIntFromRect(_atlasGridSize, _rects[_rectIndex]);

            foreach (var tex in _msData.Textures.Texs)
            {
                var colors = tex.Value.GetPixels32();
                _atlas.Textures.Texs.Where(t => t.Key == tex.Key).FirstOrDefault().Value.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, colors);
            }
            _atlas.Textures.ID = 111; // For debugging purposes
        }

        private static void CreateAtlasedMesh()
        {
            _atlasedMesh = Utils.CopyMesh(_msData.NativeMesh);
            _atlasedMesh.uv = MeshAnalyzer.RemapUVs(_atlasedMesh.uv, _rects[_rectIndex]);
        }

        private static void UpdateMateriaSetterData()
        {
            if (_msData.AtlasedMesh != null)
                AssetDatabase.RemoveObjectFromAsset(_msData.AtlasedMesh);

            AssetDatabase.AddObjectToAsset(_atlasedMesh, _msData);

            _msData.MateriaAtlas = _atlas;
            _msData.AtlasedMesh = _atlasedMesh;
            _msData.AtlasedUVRect = _rects[_rectIndex];
            _msData.AtlasedGridSize = _atlasGridSize;
        }

        private static MateriaAtlas CreateMateriaAtlasAsset(string directory, string name, MaterialData materialData, Vector2Int gridSize)
        {
            var atlas = AssetUtils.CreateScriptableObjectAsset<MateriaAtlas>(directory, name);
            atlas.MaterialData = materialData;
            atlas.GridSize = gridSize;
            atlas.Textures.CreateTextures(materialData.ShaderData.Properties, gridSize.x, gridSize.y);
            atlas.Textures.SetNames(name);
            atlas.Material = UnityEngine.Object.Instantiate(materialData.Material);
            atlas.Material.name = name;

            AssetDatabase.AddObjectToAsset(atlas.Material, atlas);
            atlas.Textures.AddTexturesToAsset(atlas);

            AssetDatabase.SaveAssets();

            atlas.Textures.ImportTextureAssets();

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