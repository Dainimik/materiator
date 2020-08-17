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

        public static void CreateAtlas(KeyValuePair<MaterialData, List<MateriaSetter>> group, string path, MateriaAtlas existingAtlas = null)
        {
            var materialData = group.Key;
            var materiaSetters = group.Value;

            // collect textures
            var texs = new Dictionary<string, List<Texture2D>>();
            foreach (var item in group.Value)
            {
                foreach (var kvp in item.MateriaSetterData.Textures.Texs)
                {
                    if (!texs.ContainsKey(kvp.Key))
                    {
                        texs.Add(kvp.Key, new List<Texture2D>() { kvp.Value });
                    }
                    else
                    {
                        if (!texs[kvp.Key].Contains(kvp.Value))
                        {
                            texs[kvp.Key].Add(kvp.Value);
                        }
                    }
                }
            }

            // create textures
            var output = new List<KeyValuePair<KeyValuePair<string, Texture2D>, Rect[]>>();
            foreach (var item in texs)
            {
                var newTex = new Texture2D(8192, 8192);
                _rects = newTex.PackTextures(item.Value.ToArray(), 0, 8192, false);
                output.Add(new KeyValuePair<KeyValuePair<string, Texture2D>, Rect[]>(new KeyValuePair<string, Texture2D>(item.Key, newTex), _rects ));
            }

            // create atlas asset
            _atlas = existingAtlas;
            if (existingAtlas == null)
                _atlas = CreateMateriaAtlasAsset(AssetUtils.GetDirectoryName(path), AssetUtils.GetFileName(path), materialData, output);

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

                            //SetAtlasTextureValues();
                            _atlas.Textures.ID = 111; // For debugging purposes

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

        private static void FillAtlasWithItems(GameObject[] prefabs, int i)
        {
            var prefabMS = prefabs[i].GetComponentsInChildren<MateriaSetter>().Where(setter => setter.MateriaSetterData == _msData).FirstOrDefault();

            if (!_atlas.AtlasItems.ContainsKey(i))
                _atlas.AtlasItems.Add(i, new MateriaAtlasItem(prefabMS, _msData));
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
        }

        private static MateriaAtlas CreateMateriaAtlasAsset(string directory, string name, MaterialData materialData, List<KeyValuePair<KeyValuePair<string, Texture2D>, Rect[]>> output)
        {
            var atlas = AssetUtils.CreateScriptableObjectAsset<MateriaAtlas>(directory, name);
            atlas.GridSize = new Vector2Int(output[0].Key.Value.width, output[0].Key.Value.height);
            _atlasGridSize = atlas.GridSize;
            atlas.MaterialData = materialData;            
            atlas.Material = UnityEngine.Object.Instantiate(materialData.Material);
            atlas.Material.name = name;
            AssetDatabase.AddObjectToAsset(atlas.Material, atlas);

            foreach (var item in output)
                atlas.Textures.Texs.Add(item.Key.Key, item.Key.Value);

            atlas.Textures.SetNames(name);
            atlas.Textures.AddTexturesToAsset(atlas);

            AssetDatabase.SaveAssets();

            atlas.Textures.ImportTextureAssets();

            return atlas;
        }
    }
}