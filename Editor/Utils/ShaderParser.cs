using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class ShaderParser
    {
        private  List<ShaderProperty> _shaderProperties = new List<ShaderProperty>();

        public static List<ShaderProperty> ProcessShader(Shader shader)
        {
            var text = File.ReadAllText(AssetDatabase.GetAssetPath(shader));

            var shaderPropertyStrings = GetShaderProperties(text);

            var shaderProperties = ParseShaderProperties(shaderPropertyStrings);

            var shaderVariableName = "";

            var vectorTypeExpression = @"\d";

            foreach (var shaderProperty in shaderProperties)
            {
                if (shaderProperty.Value.Count > 1)
                {
                    shaderVariableName = shaderProperty.PropertyName;

                    var shaderVariableExpression = @"((float[2-4]|fixed[2-4]|half[2-4])\s+" + shaderVariableName + ";)";
                    var shaderVar = ParseString(text, shaderVariableExpression)[0];
                    var floatType = ParseStringFirstMatch(shaderVar, vectorTypeExpression);

                    var count = shaderProperty.Value.Count - int.Parse(floatType);
                    for (int i = 0; i < count; i++)
                    {
                        shaderProperty.Value.RemoveAt(shaderProperty.Value.Count - 1 - i);
                    }                    
                }
            }

            return shaderProperties;
        }

        public static List<string> GetShaderProperties(string text)
        {
            var expression = @"(?s)(?=\[|_).*?\=\s\S[\d|\.|\,|\)]*";

            var propertiesText = ExtractText(text, "Properties", "SubShader");

            return ParseString(propertiesText, expression);
        }

        private static string ExtractText(string text, string from, string to)
        {
            var expression = "(?s)(?<=" + from + ").+?(?=" + to + ")";

            return ParseString(text, expression)[0];
        }

        private static List<ShaderProperty> ParseShaderProperties(List<string> properties)
        {
            string attributeExpression = @"\[(.*?)\]";
            string propertyNameExpression = @"(?=_).*?(?=\s|\("")";
            string nameExpression = "(\".*\")";
            string valueExpression = @"(\=\s*.*)";
            string rangeExpression = @"(Range.*\))";

            var results = new List<ShaderProperty>();

            foreach (var prop in properties)
            {
                if (prop.StartsWith("//"))
                {
                    continue;
                }

                var attributes = ParseString(prop, attributeExpression);

                if (attributes.Contains("[HideInInspector]"))
                {
                    continue;
                }

                string propertyName = ParseString(prop, propertyNameExpression)[0];
                string name = ParseString(prop, nameExpression)[0];

                string valueString = ParseString(prop, valueExpression)[0];
                string digitExpression = @"([-+]?[0-9]*\.?[0-9]+)";

                var value = ParseString(valueString, digitExpression);

                var type = GetPropertyType(prop);

                var rangeString = ParseString(prop, rangeExpression);
                
                Vector2 range = default;

                if (rangeString.Count > 0)
                {
                    var rangeList = ParseString(rangeString[0], digitExpression);

                    range.x = float.Parse(rangeList[0]);
                    range.y = float.Parse(rangeList[1]);
                }
                
                
                var property = new ShaderProperty(prop, propertyName, name.Substring(1, name.Length - 2), type, value, range);

                results.Add(property);
            }

            return results;
        }

        private static ShaderPropertyType GetPropertyType(string property)
        {
            string colorExpression = @"(?<=\,\s)Color(?=\))";

            var colorString = ParseString(property, colorExpression);

            if (colorString.Count > 0)
            {
                return ShaderPropertyType.Color;
            }
            else
            {
                return ShaderPropertyType.Vector;
            }

        }

        public static List<string> ParseFile(string path, string expression)
        {
            var text = File.ReadAllText(path);

            MatchCollection mc = Regex.Matches(text, expression);

            var results = new List<string>();

            foreach (var match in mc)
                results.Add(match.ToString());

            return results;
        }

        private static List<string> ParseString(string text, string expression)
        {
            MatchCollection mc = Regex.Matches(text, expression);

            var results = new List<string>();

            foreach (var match in mc)
                results.Add(match.ToString());

            return results;
        }

        private static string ParseStringFirstMatch(string text, string expression)
        {
            var match = Regex.Match(text, expression);

            return match.ToString();
        }
    }
}
