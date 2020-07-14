using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaTags))]
    public class MateriaTagsEditor : MateriatorEditor
    {
        private MateriaTags _materiaTags;

        private ReorderableList _tagsList;

        private void OnEnable()
        {
            _materiaTags = (MateriaTags)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaTags>();

            _tagsList = new ReorderableList(serializedObject, serializedObject.FindProperty("MateriaTagsList"), true, true, true, true);
            SetUpTagList();

            IMGUIContainer materiaTagsReorderableListContainer = new IMGUIContainer(() => ExecuteIMGUI());
            root.Add(materiaTagsReorderableListContainer);

            return root;
        }

        private void ExecuteIMGUI()
        {
            serializedObject.Update();
            _tagsList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void SetUpTagList()
        {
            _tagsList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, new GUIContent("Tag", "Tag"), EditorStyles.boldLabel);
            };

            _tagsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _tagsList.serializedProperty.GetArrayElementAtIndex(index);
                Rect r = new Rect(rect.x, rect.y, 150f, 22f);

                EditorGUI.BeginChangeCheck();
                var value = EditorGUI.TextField(rect, element.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!_materiaTags.MateriaTagsList.ConvertAll(s => s.ToLower()).Contains(value.ToLower()))
                    {
                        element.stringValue = value;
                    }
                    else
                    {
                        element.stringValue = "";
                    }
                }
            };

            _tagsList.onAddCallback = (ReorderableList List) =>
            {
                _materiaTags.MateriaTagsList.Add("");
            };
        }
    }
}