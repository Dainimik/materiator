using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public static class IOUtils
    {
        public static Shader CreateTextFile(ShaderData shaderData)
        {
            var shaderDataPath = AssetDatabase.GetAssetPath(shaderData);
            var sourceShaderPath = AssetDatabase.GetAssetPath(shaderData.SourceShader);

            var shader = shaderData.Shader;
            var shaderFileName = AssetUtils.GetFileName(AssetDatabase.GetAssetPath(shaderData.Shader)) + "_Materiator.shader";
            var shaderPath = AssetDatabase.GetAssetPath(shader);

            if (!shader)
            {
                /*var assets = AssetDatabase.LoadAllAssetsAtPath(shaderDataPath);
                if (assets.Length > 0)
                    foreach (var asset in assets)
                        if (AssetDatabase.IsSubAsset(asset))
                            AssetDatabase.RemoveObjectFromAsset(asset);*/

                shader = UnityEngine.Object.Instantiate(shaderData.SourceShader);
                shader.name = shaderFileName;

                AssetDatabase.AddObjectToAsset(shader, shaderData);
                shaderPath = AssetDatabase.GetAssetPath(shader);
                AssetDatabase.ImportAsset(shaderPath);
            }

            var shaderNamePattern = "Shader\\s?\"" + shaderData.SourceShader.name + "\"";

            ReplaceInFile(Path.GetFullPath(shaderPath), shaderNamePattern, "Shader \"Materiator/" + shaderData.SourceShader.name + "\"");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //replace
            //shaderSourceCode = shaderSourceCode.Replace("[FUNCTION]", function);

            return AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GetAssetPath(shader));
        }

        /// <summary>
        /// Replaces text in a file.
        /// </summary>
        /// <param name="filePath">Path of the text file.</param>
        /// <param name="searchText">Text to search for.</param>
        /// <param name="replaceText">Text to replace the search text with.</param>
        static public void ReplaceInFile(string filePath, string searchText, string replaceText)
        {
            StreamReader reader = new StreamReader(filePath);
            string content = reader.ReadToEnd();
            reader.Close();

            content = content.Replace(searchText, replaceText);

            StreamWriter writer = new StreamWriter(filePath);
            writer.Write(content);
            writer.Close();
        }

        static public void ReplaceInString(string filePath, string searchText, string replaceText)
        {
            StreamReader reader = new StreamReader(filePath);
            string content = reader.ReadToEnd();
            reader.Close();

            content = content.Replace(searchText, replaceText);

            StreamWriter writer = new StreamWriter(filePath);
            writer.Write(content);
            writer.Close();
        }

        static public void WriteToFile(string filePath, string text)
        {
            StreamReader reader = new StreamReader(filePath);
            string content = reader.ReadToEnd();
            reader.Close();

            //content = content.Replace(searchText, replaceText);

            StreamWriter writer = new StreamWriter(filePath);
            writer.Write(content);
            writer.Close();
        }

        /// <summary>
        /// Inserts a string in a text file
        /// </summary>
        /// <param name="text">String to insert</param>
        /// <param name="expression">Regex to find a match after which the new text should be inserted</param>
        /// <param name="filePath">Text file absolute path</param>
        public static void InsertStringAfter(string text, string expression, string filePath)
        {
            StreamReader reader = new StreamReader(filePath);
            string content = reader.ReadToEnd();
            reader.Close();

            var matches = Regex.Matches(content, expression);
            var newContent = "";
            var split = new List<string>();

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i].ToString();
                if (!split.Contains(match))
                    split.Add(match);
            }

            var splitContent = content.Split(split.ToArray(), StringSplitOptions.None);

            for (int i = 0; i < splitContent.Length; i++)
            {
                if (i < splitContent.Length - 1)
                    newContent += splitContent[i] + split[0] + text;
                else
                    newContent += splitContent[i];
            }

            StreamWriter writer = new StreamWriter(filePath);
            writer.Write(newContent);
            writer.Close();
        }

        public static void InsertStringBefore(string text, string expression, string filePath)
        {
            StreamReader reader = new StreamReader(filePath);
            string content = reader.ReadToEnd();
            reader.Close();

            var matches = Regex.Matches(content, expression);
            var newContent = "";
            var split = new List<string>();

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i].ToString();
                if (!split.Contains(match))
                    split.Add(match);
            }

            var splitContent = content.Split(split.ToArray(), StringSplitOptions.None);

            for (int i = 0; i < splitContent.Length; i++)
            {
                if (i < splitContent.Length - 1)
                    newContent += splitContent[i] + text + split[0];
                else
                    newContent += splitContent[i];
            }

            StreamWriter writer = new StreamWriter(filePath);
            writer.Write(newContent);
            writer.Close();
        }
    }
}