using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaSetter))]
    public class MateriaSetterEditor : MateriatorEditor
    {
        public Action OnTagChanged;

        public MateriaSetter MateriaSetter;

        public AtlasSection AtlasSection;
        private ReorderableList _materiaReorderableList;

        public VisualElement Root;
        private VisualElement _IMGUIContainer;

        public Mesh OriginalMesh;
        public Mesh InstanceMesh;

        private void OnEnable()
        {
            MateriaSetter = (MateriaSetter)target;

            InitializeEditor<MateriaSetter>();

            Root = new VisualElement();

            if (Initialize())
            {
                AtlasSection = new AtlasSection(this);

                DrawDefaultGUI();
            }
        }

        private void DrawDefaultGUI()
        {
            IMGUIContainer defaultInspector = new IMGUIContainer(() => DrawDefaultInspector());
            root.Add(defaultInspector);

            DrawIMGUI();
        }

        public override VisualElement CreateInspectorGUI()
        {
            return Root;
        }

        public bool Initialize()
        {
            if (SystemChecker.CheckAllSystems(this))
            {
                MateriaSetter.Initialize();
                SetUpMeshes();

                if (MateriaSetter.MateriaAtlas != null)
                    MateriaSetter.LoadAtlas(MateriaSetter.MateriaAtlas, InstanceMesh);

                Root = root;

                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
                DestroyImmediate(InstanceMesh);
        }

        private void SetUpMeshes()
        {
            OriginalMesh = MateriaSetter.Mesh;
            InstanceMesh = OriginalMesh;

            if (!Application.isPlaying)
            {
                InstanceMesh = MeshUtils.CopyMesh(MateriaSetter.Mesh);
                InstanceMesh.hideFlags = HideFlags.HideAndDontSave;
            }
            
        }

        private void DrawIMGUI()
        {
            _materiaReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("MateriaSetterSlots"), false, true, false, false);
            DrawMateriaReorderableList();
            IMGUIContainer materiaReorderableList = new IMGUIContainer(() => MateriaReorderableList());
            _IMGUIContainer.Add(materiaReorderableList);

            IMGUIContainer preview = new IMGUIContainer(() => HandlePreview());
            _IMGUIContainer.Add(preview);
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
                EditorGUI.LabelField(new Rect(rect.x + 170f, rect.y, 100f, 20f), new GUIContent("Slot Name"), EditorStyles.boldLabel);
            };

            _materiaReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                serializedObject.Update();
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(index);
                var elementID = element.FindPropertyRelative("ID");
                var elementTag = element.FindPropertyRelative("Tag").objectReferenceValue as MateriaTag;
                var materiaTag = MateriaSetter.MateriaSetterSlots[index].Tag;

                Rect r = new Rect(rect.x, rect.y, 22f, 22f);

                serializedObject.Update();

                EditorGUI.BeginChangeCheck();
                elementTag = (MateriaTag)EditorGUI.ObjectField(new Rect(rect.x + 25f, rect.y, 95f, rect.height), elementTag, typeof(MateriaTag), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(MateriaSetter, "Change Materia Tag");

                    MateriaSetter.MateriaSetterSlots[index].Tag = elementTag;

                    serializedObject.Update();
                    serializedObject.ApplyModifiedProperties();

                    OnTagChanged?.Invoke();
                }

                EditorGUI.LabelField(new Rect(rect.x + 170f, rect.y, rect.width - 195f, rect.height), MateriaSetter.MateriaSetterSlots[index].Name);

                Rect cdExpandRect = new Rect(EditorGUIUtility.currentViewWidth - 70f, rect.y, 20f, 20f);
            };
        }

        protected override void GetProperties()
        {
            _IMGUIContainer = root.Q<VisualElement>("IMGUIContainer");
        }

        protected override void SetUpView()
        {
            //
        }

        protected override void BindProperties()
        {
            //
        }

        protected override void RegisterCallbacks()
        {
            //
        }

        private void HandlePreview()
        {
            if (GUILayout.RepeatButton(new GUIContent("Preview")))
            {
                MeshUtils.SetSharedMesh(InstanceMesh, MateriaSetter.gameObject);
                MateriaSetter.Mesh = InstanceMesh;
            }
            else
            {
                MeshUtils.SetSharedMesh(OriginalMesh, MateriaSetter.gameObject);
                MateriaSetter.Mesh = OriginalMesh;
            }
        }
    }
}
