using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaPreset))]
    public class MateriaPresetEditor : MateriatorEditor
    {
        private MateriaPreset _materiaPreset;

        private ReorderableList _tagList;

        private void OnEnable()
        {
            _materiaPreset = (MateriaPreset)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaPreset>();

            _tagList = new ReorderableList(serializedObject, serializedObject.FindProperty("MateriaPresetItemList"), true, true, true, true);
            SetUpTagListUI();

            IMGUIContainer materiaTagsReorderableList = new IMGUIContainer(() => ExecuteIMGUI());
            root.Add(materiaTagsReorderableList);

            return root;
        }

        private void ExecuteIMGUI()
        {
            serializedObject.Update();
            _tagList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void SetUpTagListUI()
        {
            _tagList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(new Rect(rect.x + 25f, rect.y, 50f, 20f), new GUIContent("Tag", "Tag"), EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + 200f, rect.y, 200f, 20f), new GUIContent("Materia", "Materia"), EditorStyles.boldLabel);
            };

            _tagList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _tagList.serializedProperty.GetArrayElementAtIndex(index);
                var materiaTag = element.FindPropertyRelative("Tag");
                var materia = element.FindPropertyRelative("Materia").objectReferenceValue as Materia;

                Rect r = new Rect(rect.x, rect.y, 150f, 22f);

                EditorGUI.PropertyField(new Rect(r.x + 15, r.y, r.width, r.height), element.FindPropertyRelative("Tag"), GUIContent.none);
                EditorGUI.PropertyField(new Rect(r.x + 185, r.y, rect.width - 175f, r.height), element.FindPropertyRelative("Materia"), GUIContent.none);

                //materiaTag.stringValue = EditorGUI.Popup(new Rect(rect.x + 25f, rect.y, rect.width - 300f, rect.height), materiaTag.intValue, _materiaTagArray, EditorStyles.popup);
                if (EditorGUI.EndChangeCheck())
                {
                    var canSetTag = true;
                    

                    //_materiaTagArray = Utils.ReloadTagList();
                }

                //EditorGUI.PropertyField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Type"), GUIContent.none);
            };
        }
    }
}