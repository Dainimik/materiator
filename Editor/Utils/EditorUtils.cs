using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Materiator
{
    public static class EditorUtils
    {
        /// <summary>
        /// Creates a new inspector window instance and locks it to inspect the specified target
        /// </summary>
        public static void InspectTarget(Object target)
        {
            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            var inspectorInstance = ScriptableObject.CreateInstance(inspectorType) as EditorWindow;
            inspectorInstance.Show();
            var prevSelection = Selection.activeGameObject;
            Selection.activeObject = target;
            var isLocked = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);
            isLocked.GetSetMethod().Invoke(inspectorInstance, new object[] { true });
            Selection.activeGameObject = prevSelection;
        }

        public static void MarkOpenPrefabSceneDirty()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }

        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && position.width > 50f)
                    {
                        GUIUtility.hotControl = controlID;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                    }
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        scrollPosition -= current.delta * (float)((!current.shift) ? 1 : 3) / Mathf.Min(position.width, position.height) * 140f;
                        scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
                        current.Use();
                        GUI.changed = true;
                    }
                    break;
            }
            return scrollPosition;
        }

        public static void GenerateMateriaPreviewIcons(Materia materia, Material material)
        {
            if (materia.PreviewIcon != null)
                Object.DestroyImmediate(materia.PreviewIcon);

            /*material.SetColor(SystemData.Settings.DefaultShaderData.BaseColorPropertyName, materia.BaseColor);
            material.SetFloat("_Metallic", materia.Metallic);
            material.SetFloat("_Smoothness", materia.Smoothness);
            if (materia.IsEmissive)
            {
                material.EnableKeyword(SystemData.Settings.DefaultShaderData.EmissionKeywordName);
                //material.globalIlluminationFlags = SystemData.Settings.GlobalIlluminationFlag;
                material.SetColor(SystemData.Settings.DefaultShaderData.EmissionColorPropertyName, materia.EmissionColor);
            }
            else
            {
                material.DisableKeyword(SystemData.Settings.DefaultShaderData.EmissionKeywordName);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }*/

            materia.PreviewIcon = GenerateThumbnail(material, Color.black);
        }

        public static Texture2D GenerateThumbnail(Material material, Color backgroundColor)
        {
            RuntimePreviewGenerator.BackgroundColor = backgroundColor;
            RuntimePreviewGenerator.PreviewDirection = new Vector3(0f, 0f, 1f);
            RuntimePreviewGenerator.Padding = -0.125f;
            var tex = RuntimePreviewGenerator.GenerateMaterialPreview(material, PrimitiveType.Sphere, 128, 128);
            return tex;
        }
    }
}