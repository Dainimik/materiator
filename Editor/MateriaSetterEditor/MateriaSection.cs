using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    public class MateriaSection
    {
        private MateriaAtlasEditor _editor;
        private MateriaAtlas _materiaAtlas;

        private ReorderableList _materiaReorderableList;

        private VisualElement _IMGUIContainer;

        private string _serializedProperty;

        public MateriaSection(MateriatorEditor editor, string serializedProperty = "")
        {
            _editor = editor as MateriaAtlasEditor;
            _materiaAtlas = _editor.MateriaAtlas;
            _serializedProperty = serializedProperty;
            _IMGUIContainer = _editor.IMGUIContainer;

            DrawIMGUI();
        }

        private void DrawIMGUI()
        {
            _materiaReorderableList = new ReorderableList(_editor.serializedObject, _editor.serializedObject.FindProperty(_serializedProperty), true, true, true, true);
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
                EditorGUI.LabelField(new Rect(rect.x + 135f, rect.y, 20f, 20f), new GUIContent("C", "Base Color"), EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + 170f, rect.y, 100f, 20f), new GUIContent("Materia " + "(" + _materiaAtlas.MateriaSlots.Count + ")"), EditorStyles.boldLabel);
            };

            _materiaReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                _editor.serializedObject.Update();
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(index);
                var elementID = index;
                var elementMateria = element.FindPropertyRelative("Materia").objectReferenceValue as Materia;
                var elementTag = element.FindPropertyRelative("Tag").objectReferenceValue as MateriaTag;
                var materiaTag = _materiaAtlas.MateriaSlots[index].Tag;

                Rect r = new Rect(rect.x, rect.y, 22f, 22f);

                var tex = new Texture2D(4, 4);

                EditorGUI.LabelField(r, new GUIContent((index + 1).ToString()));
                EditorGUI.LabelField(new Rect(rect.x + 130f, rect.y, rect.width, rect.height), new GUIContent(_materiaAtlas.MateriaSlots[index].Materia.PreviewIcon));

                _editor.serializedObject.Update();

                int _materiaTagIndex = 0;
                EditorGUI.BeginChangeCheck();
                elementTag = (MateriaTag)EditorGUI.ObjectField(new Rect(rect.x + 25f, rect.y, 95f, rect.height), elementTag, typeof(MateriaTag), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(_materiaAtlas, "Change Materia Tag");
                    //_editor.SetMateriaSetterDirty(true);hja

                    if (elementTag == null)
                        elementTag = SystemData.Settings.DefaultTag;
                    else
                        _materiaAtlas.MateriaSlots[index].Tag = elementTag;

                    _editor.serializedObject.ApplyModifiedProperties();
                    //_editor.OnMateriaSetterUpdated?.Invoke();
                    //_emissionInUse = IsEmissionInUse(_materiaSetter.Materia);
                }
                //_editor.SetMateriaSetterDirty(true);

                EditorGUI.BeginChangeCheck();
                elementMateria = (Materia)EditorGUI.ObjectField(new Rect(rect.x + 170f, rect.y, rect.width - 195f, rect.height), elementMateria, typeof(Materia), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(_materiaAtlas, "Change Materia");
                    //_editor.SetMateriaSetterDirty(true);hja

                    if (elementMateria == null)
                        elementMateria = SystemData.Settings.DefaultMateria;
                    else
                        _materiaAtlas.MateriaSlots[index].Materia = elementMateria;

                    _editor.serializedObject.Update();

                    //_materiaAtlas.UpdateColorsOfAllTextures();

                    _editor.serializedObject.ApplyModifiedProperties();
                    //_editor.OnMateriaSetterUpdated?.Invoke();
                    //_emissionInUse = IsEmissionInUse(_materiaSetter.Materia);
                }

                Rect cdExpandRect = new Rect(EditorGUIUtility.currentViewWidth - 70f, rect.y, 20f, 20f);
                if (GUI.Button(cdExpandRect, new GUIContent(EditorGUIUtility.IconContent("d_editicon.sml").image, "Edit Color Data")))
                    EditorUtils.InspectTarget(elementMateria);
            };
        }
    }
}