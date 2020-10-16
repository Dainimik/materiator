using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    public class ShaderParser : MonoBehaviour
    {
        public Shader Shader;

        private struct ShaderProperty
        {
            public string PropertyName;
            public string Name;
            public List<string> Value;
            public Vector2 Range;

            public ShaderProperty(string propertyName, string name, List<string> value, Vector2 range = default)
            {
                PropertyName = propertyName;
                Name = name;
                Value = value;
                Range = range;
            }
        }

        private  List<ShaderProperty> _shaderProperties = new List<ShaderProperty>();

        [ContextMenu("ProcessShader")]
        public void ProcessShader()
        {
            var text = File.ReadAllText(AssetDatabase.GetAssetPath(Shader));

            var shaderProperties = GetShaderProperties(text);

            _shaderProperties = ParseShaderProperties(shaderProperties);

            var textureCount = GetRequiredTextureCount(_shaderProperties);

            Debug.Log("TexCount: " + textureCount);
        }

        private List<string> GetShaderProperties(string text)
        {
            var expression = @"\S.*\w*[\s\S]\(\"".*\=.*";

            return ParseString(text, expression);
        }

        private List<ShaderProperty> ParseShaderProperties(List<string> properties)
        {
            //char[] separators = { '(', '"', '=' };

            string attributeExpression = @"(\[[a-zA-Z]*\.?[a-zA-Z]\])";
            string propertyNameExpression = @"(.+?(?=\())";
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
                foreach (var item in attributes)
                {
                    Debug.Log(item);
                }
                if (attributes.Contains("[HideInInspector]"))
                {
                    continue;
                }

                string propertyName = ParseString(prop, propertyNameExpression)[0];
                string name = ParseString(prop, nameExpression)[0];

                string valueString = ParseString(prop, valueExpression)[0];
                string digitExpression = @"([-+]?[0-9]*\.?[0-9]+)";

                var value = ParseString(valueString, digitExpression);
                
                var rangeString = ParseString(prop, rangeExpression);
                
                Vector2 range = default;

                if (rangeString.Count > 0)
                {
                    var rangeList = ParseString(rangeString[0], digitExpression);

                    range.x = float.Parse(rangeList[0]);
                    range.y = float.Parse(rangeList[1]);
                }
                
                
                var property = new ShaderProperty(propertyName, name.Substring(1, name.Length - 2), value, range);

                results.Add(property);

                Debug.Log(property.PropertyName);
                Debug.Log(property.Name);
                Debug.Log(property.Value);
                Debug.Log(property.Range);

            }

            return results;
        }

        private List<string> ParseString(string text, string expression)
        {
            MatchCollection mc = Regex.Matches(text, expression);

            var results = new List<string>();

            foreach (var match in mc)
                results.Add(match.ToString());

            return results;
        }

        private int GetRequiredTextureCount(List<ShaderProperty> shaderProperties)
        {
            var count = 0;

            foreach (var prop in shaderProperties)
            {
                count += prop.Value.Count;
            }

            return Mathf.CeilToInt(count / 4);
        }
    }
}
