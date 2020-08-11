using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Materiator
{
    [CustomPropertyDrawer(typeof(ShaderPropertyAttribute))]
    public class ShaderPropertyDrawer : PropertyDrawer
    {
        private Type[] _properties;
        private int _propertyTypeIndex;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            //if (_properties == null || GUI.Button(new Rect(position.x, position.y, position.width, position.height), "Refresh properties"))
            if (_properties == null)
            {
                _properties = GetMateriaProperties((attribute as ShaderPropertyAttribute).FieldType)
                    .Where(prop => !prop.IsSubclassOf(typeof(UnityEngine.Object))).ToArray();
            }

            if (property.managedReferenceFullTypename != "")
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, position.height), property, true);
            }
            else
            {
                _propertyTypeIndex = EditorGUI.Popup(new Rect(position.x, position.y, position.width, position.height), "Property Type", _propertyTypeIndex, _properties.Select(impl => impl.FullName).ToArray());

                if (GUI.Button(new Rect(position.x, position.y + 25f, position.width, 20f), "Create instance"))
                {
                    property.managedReferenceValue = Activator.CreateInstance(_properties[_propertyTypeIndex]);
                }
            }

            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public static Type[] GetMateriaProperties(Type interfaceType)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
            return types.Where(p => interfaceType.IsAssignableFrom(p) && !p.IsAbstract).ToArray();
        }
    }
}