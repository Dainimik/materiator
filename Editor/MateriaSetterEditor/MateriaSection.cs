using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    public class MateriaSection
    {
        private MateriaSetterEditor _editor;
        private MateriaSetter _materiaSetter;

        private ReorderableList _materiaReorderableList;

        private VisualElement _IMGUIContainer;

        public MateriaSection(MateriaSetterEditor editor)
        {
            _editor = editor;
            _materiaSetter = editor.MateriaSetter;

            GetProperties();

            DrawIMGUI();
        }

        private void DrawIMGUI()
        {
            _materiaReorderableList = new ReorderableList(_editor.serializedObject, _editor.serializedObject.FindProperty("MateriaSlots"), false, true, false, false);
            DrawMateriaReorderableList();
            IMGUIContainer materiaReorderableList = new IMGUIContainer(() => MateriaReorderableList());
            _IMGUIContainer.Add(materiaReorderableList);
        }

        private void MateriaReorderableList()
        {
            _materiaReorderableList.DoLayoutList();
        }

        public void DrawMateriaReorderableList()
        {
            _materiaReorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(new Rect(rect.x + 25f, rect.y, 50f, 20f), new GUIContent("Tag", "Tag"), EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + 125f, rect.y, 20f, 20f), new GUIContent("C", "Base Color"), EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + 150f, rect.y, 20f, 20f), new GUIContent("E", "Emission Color"), EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + 170f, rect.y, 100f, 20f), new GUIContent("Materia " + "(" + _materiaSetter.MateriaSlots.Count + ")"), EditorStyles.boldLabel);
            };

            _materiaReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                _editor.serializedObject.Update();
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(index);
                var elementID = element.FindPropertyRelative("ID");
                var elementMateria = element.FindPropertyRelative("Materia").objectReferenceValue as Materia;
                var materiaTag = _materiaSetter.MateriaSlots[index].Tag;

                Rect r = new Rect(rect.x, rect.y, 22f, 22f);

                var color = elementMateria.BaseColor;
                var emission = elementMateria.EmissionColor;

                color.a = 255;
                emission.a = 255;

                if (elementMateria.IsEmissive)
                {
                    var texE = new Texture2D(4, 4);
                    Rect texRE = new Rect(rect.x + 145f, rect.y, 20f, 20f);
                    GUI.DrawTexture(texRE, texE, ScaleMode.StretchToFill, false, 0, emission, 0, 0);
                }

                var tex = new Texture2D(4, 4);

                EditorGUI.LabelField(r, new GUIContent((elementID.intValue + 1).ToString()));
                EditorGUI.LabelField(new Rect(rect.x + 120f, rect.y, rect.width, rect.height), new GUIContent(_materiaSetter.MateriaSlots[index].Materia.PreviewIcon));

                _editor.serializedObject.Update();

                int _materiaTagIndex = 0;
                EditorGUI.BeginChangeCheck();
                _materiaTagIndex = EditorGUI.Popup(new Rect(rect.x + 25f, rect.y, 95f, rect.height), SystemData.Settings.MateriaTags.MateriaTagsList.IndexOf(SystemData.Settings.MateriaTags.MateriaTagsList.Where(t => t.Name == materiaTag.Name).FirstOrDefault()), SystemData.Settings.MateriaTags.MateriaTagNamesArray, EditorStyles.popup);
                if (EditorGUI.EndChangeCheck())
                {
                    _editor.SetMateriaSetterDirty(true);
                    var newTag = SystemData.Settings.MateriaTags.MateriaTagsList[_materiaTagIndex];
                    Undo.RegisterCompleteObjectUndo(_materiaSetter, "Change Materia Tag");
                    _materiaSetter.MateriaSlots[index].Tag = newTag;
                    _editor.serializedObject.Update();
                    _editor.OnMateriaSetterUpdated?.Invoke();
                }

                EditorGUI.BeginChangeCheck();
                elementMateria = (Materia)EditorGUI.ObjectField(new Rect(rect.x + 170f, rect.y, rect.width - 195f, rect.height), elementMateria, typeof(Materia), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(_materiaSetter, "Change Materia");
                    _editor.SetMateriaSetterDirty(true);

                    if (elementMateria == null)
                        elementMateria = SystemData.Settings.DefaultMateria;
                    else
                        _materiaSetter.MateriaSlots[index].Materia = elementMateria;

                    _editor.serializedObject.Update();
                    _editor.UVInspector.DrawUVInspector(true);

                    _materiaSetter.UpdateColorsOfAllTextures();

                    _editor.serializedObject.ApplyModifiedProperties();
                    _editor.OnMateriaSetterUpdated?.Invoke();
                    //_emissionInUse = IsEmissionInUse(_materiaSetter.Materia);
                }

                Rect cdExpandRect = new Rect(EditorGUIUtility.currentViewWidth - 70f, rect.y, 20f, 20f);
                if (GUI.Button(cdExpandRect, new GUIContent(EditorGUIUtility.IconContent("d_editicon.sml").image, "Edit Color Data")))
                    EditorUtils.InspectTarget(elementMateria);
            };

            _materiaReorderableList.onSelectCallback = (ReorderableList list) =>
            {
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("ID");

                HandleMateriaSlotSelection(element.intValue, true);
            };

            _materiaReorderableList.onMouseUpCallback = (ReorderableList list) =>
            {
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("ID");
                //if (Utils.Settings.HighlightMode == HighlightMode.WhileLMBHeld) Reload();

                HandleMateriaSlotSelection(element.intValue, false);

            };
        }

        Texture2D _highlightedTexture;
        private void HandleMateriaSlotSelection(int index, bool selected)
        {
            var originalTexture = _materiaSetter.Textures.Color;

            if (_highlightedTexture == null)
            {
                _highlightedTexture = new Texture2D(_materiaSetter.Textures.Color.width, _materiaSetter.Textures.Color.height, TextureFormat.RGBA32, false);
                EditorUtility.CopySerialized(originalTexture, _highlightedTexture);
                _highlightedTexture.filterMode = SystemData.Settings.FilterMode;
            }

            var colors = originalTexture.GetPixels32();
            var rectInt = Utils.GetRectIntFromRect(_materiaSetter.GridSize, _materiaSetter.FilteredRects[index]);

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = SystemData.Settings.HighlightColor;
            }

            if (selected)
            {
                _highlightedTexture.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, colors);
                _highlightedTexture.Apply();

                _materiaSetter.Renderer.sharedMaterial.SetTexture(_materiaSetter.MaterialData.ShaderData.MainTexturePropertyName, _highlightedTexture);
            }
            else
            {
                _highlightedTexture.SetPixels32(originalTexture.GetPixels32());
                _highlightedTexture.Apply();

                _materiaSetter.Renderer.sharedMaterial.SetTexture(_materiaSetter.MaterialData.ShaderData.MainTexturePropertyName, originalTexture);
            }
        }

        private void GetProperties()
        {
            var root = _editor.Root;

            _IMGUIContainer = root.Q<VisualElement>("IMGUIContainer");
        }
    }
}