using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace Materiator
{
    public static class AssetUtils
    {
        public static T CreateOrReplaceScriptableObjectAsset<T>(T asset, string path, out bool wasExisting) where T : ScriptableObject
        {
            wasExisting = false;
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (existingAsset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
                existingAsset = asset;
                EditorUtility.SetDirty(asset);
                AssetDatabase.ImportAsset(path);
            }
            else
            {
                EditorUtility.CopySerialized(asset, existingAsset);
                wasExisting = true;
            }

            return existingAsset;
        }

        public static T CreateScriptableObjectAsset<T>(string directory, string name) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, directory + "/" + name + ".asset");
            EditorUtility.SetDirty(asset);
            AssetDatabase.ImportAsset(directory + "/" + name + ".asset");
            return AssetDatabase.LoadAssetAtPath<T>(directory + "/" + name + ".asset");
        }

        public static string GetFileName(string path, bool includeExtension = false)
        {
            string[] splitPath = path.Split(char.Parse("/"));
            if (includeExtension)
            {
                return splitPath[splitPath.Length - 1];
            }
            else
            {
                var presetNameFrag = splitPath[splitPath.Length - 1].Split(char.Parse("."));
                return presetNameFrag[0];
            }
        }

        public static string GetFileExtension(string filePathOrName, bool includeDot)
        {
            string[] splitPath = filePathOrName.Split(char.Parse("/"));
            var presetNameFrag = splitPath[splitPath.Length - 1].Split(char.Parse("."));
            if (includeDot)
                return "." + presetNameFrag[1];
            else
                return presetNameFrag[1];
        }

        /// <summary>
        /// Returns directory name with a path to it.
        /// </summary>
        /// <param name="directory">Absolute or relative directory.</param>
        /// <returns></returns>
        public static string GetDirectoryName(string directory)
        {
            return Path.GetDirectoryName(directory).Replace('\\', '/');
        }

        /// <summary>
        /// Checks if a folder exists in a given parent directory.
        /// </summary>
        /// <param name="parentFolder"> Directory where to check for a folder. String must not end with a forward slash.</param>
        /// <param name="folderInQuestion"> Folder to check for.</param>
        public static void CheckDirAndCreate(string parentFolder, string folderInQuestion)
        {
            if (!Directory.Exists(parentFolder + "/" + folderInQuestion))
                AssetDatabase.CreateFolder(parentFolder, folderInQuestion);
        }

        /// <summary>
        /// Returns a list of components from all Prefabs from a given directory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="directory"> Relative prefab directory. If null, scans project Assets folder and deeper.</param>
        /// <returns></returns>
        public static List<T> FindAllComponentsInPrefabs<T>(string[] searchDirectories = null) where T : MonoBehaviour
        {
            List<T> objects = new List<T>();

            var prefabs = FindAssets<GameObject>(searchDirectories);
            foreach (var prefab in prefabs)
            {
                var components = prefab.GetComponentsInChildren<T>();
                foreach (var comp in components)
                    objects.Add(comp);
            }

            return objects;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"> Type of asset to look for.</typeparam>
        /// <param name="searchDirectories">If null, searches the whole project starting from the Assets folder.</param>
        /// <returns></returns>
        public static List<T> FindAssets<T>(string[] searchDirectories = null) where T : Object
        {
            if (searchDirectories == null)
                searchDirectories = new string[] { "Assets" };

            var assets = new List<T>();
            var assetGUIDs = AssetDatabase.FindAssets("t:" + typeof(T).Name, searchDirectories);
            foreach (var guid in assetGUIDs)
                assets.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));

            return assets;
        }

        public static T LoadAssetFromUniqueAssetPath<T>(string aAssetPath) where T : UnityEngine.Object
        {
            if (aAssetPath.Contains("::"))
            {
                string[] parts = aAssetPath.Split(new string[] { "::" }, System.StringSplitOptions.RemoveEmptyEntries);
                aAssetPath = parts[0];
                if (parts.Length > 1)
                {
                    string assetName = parts[1];
                    System.Type t = typeof(T);
                    var assets = AssetDatabase.LoadAllAssetsAtPath(aAssetPath)
                        .Where(i => t.IsAssignableFrom(i.GetType())).Cast<T>();
                    var obj = assets.Where(i => i.name == assetName).FirstOrDefault();
                    if (obj == null)
                    {
                        int id;
                        if (int.TryParse(parts[1], out id))
                            obj = assets.Where(i => i.GetInstanceID() == id).FirstOrDefault();
                    }
                    if (obj != null)
                        return obj;
                }
            }
            return AssetDatabase.LoadAssetAtPath<T>(aAssetPath);
        }

        public static T[] FindObjectsOfTypeInOpenedPrefabStage<T>(out PrefabStage prefabStage)
        {
            var ps = PrefabStageUtility.GetCurrentPrefabStage();
            prefabStage = ps;
            if (ps != null)
            {
                var rootGameObjects = ps.scene.GetRootGameObjects();
                T[] objs = new T[rootGameObjects.Length];
                for (int i = 0; i < rootGameObjects.Length; i++)
                {
                    var obj = rootGameObjects[i].GetComponent<T>();
                    if (obj != null) objs[i] = obj;
                }
                return objs;
            }
            return null;
        }
    }
}