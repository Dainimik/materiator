using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaSetter))]
    public class MateriaSetterEditor : MateriatorEditor
    {
        private MateriaSetter _materiaSetter;

        private ReorderableList _materiaReorderableList;
        private string[] category;

        private Button _reloadButton;

        private void OnEnable()
        {
            _materiaSetter = (MateriaSetter)target;

            Initialize();
            ReloadList();
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaSetter>();

            IMGUIContainer defaultInspector = new IMGUIContainer(() => DrawDefaultInspector());
            root.Add(defaultInspector);


            _materiaReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("MateriaIndices"), false, true, false, false);
            DrawMateriaReorderableList();
            IMGUIContainer materiaReorderableList = new IMGUIContainer(() => MateriaReorderableList());
            root.Add(materiaReorderableList);

            SetUpButtons();

            return root;
        }

        private void Initialize()
        {
            _materiaSetter.Initialize();

            /*if (!CheckAllSystems())
            {
                return;
            }*/

            GenerateMateriaDictionary();

            _materiaSetter.UpdateTexturePixelColors();

            _materiaSetter.MateriaIndices = new int[_materiaSetter.Materia.Count];
            _materiaSetter.MateriaIndices = _materiaSetter.Materia.Keys.ToArray();
        }

        private void MateriaReorderableList()
        {
            _materiaReorderableList.DoLayoutList();
        }

        private void ReloadList()
        {
            category = Utils.MateriaCategories.MateriaCategoriesDictionary.Values.ToArray();
        }

        private void DrawMateriaReorderableList()
        {
            _materiaReorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(new Rect(rect.x + 25f, rect.y, 50f, 20f), new GUIContent("CAT", "Category"), EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + 125f, rect.y, 20f, 20f), new GUIContent("C", "Base Color"), EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + 150f, rect.y, 20f, 20f), new GUIContent("E", "Emission Color"), EditorStyles.boldLabel);
                EditorGUI.LabelField(new Rect(rect.x + 170f, rect.y, 100f, 20f), new GUIContent("Materia " + "(" + _materiaSetter.Materia.Count + ")"), EditorStyles.boldLabel);
            };

            _materiaReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(index);
                Rect r = new Rect(rect.x, rect.y, 22f, 22f);

                if (!_materiaSetter.Materia.ContainsKey(element.intValue))
                {
                    EditorGUI.LabelField(r, new GUIContent((element.intValue + 1).ToString()));
                    EditorGUI.LabelField(new Rect(rect.x + 70f, rect.y, rect.width - 95f, rect.height), "Create New Preset to Unlock Editing");
                    return;
                }

                var color = _materiaSetter.Materia[element.intValue].BaseColor;
                var emission = _materiaSetter.Materia[element.intValue].EmissionColor;

                color.a = 255;
                emission.a = 1f;

                if (_materiaSetter.Materia[element.intValue].IsEmissive)
                {
                    var texE = new Texture2D(4, 4);
                    Rect texRE = new Rect(rect.x + 45f, rect.y, 20f, 20f);
                    GUI.DrawTexture(texRE, texE, ScaleMode.StretchToFill, false, 0, emission, 0, 0);
                }

                var tex = new Texture2D(4, 4);
                //Rect r = new Rect(rect.x, rect.y, 20f, 20f);
                //GUI.DrawTexture(r, tex, ScaleMode.StretchToFill, false, 0, color, 0, 0);

                EditorGUI.LabelField(r, new GUIContent((element.intValue + 1).ToString()));
                //EditorGUI.LabelField(new Rect(rect.x + 20f, rect.y, rect.width, rect.height), new GUIContent(_materiaSetter.Materia[element.intValue].PreviewIconGray));

                EditorGUI.BeginChangeCheck();
                //EditorGUI.PropertyField(new Rect(rect.x + 50f, rect.y, rect.width - 75f, rect.height), element, GUIContent.none);

                //_materiaSetter.MateriaCategory[element.intValue] = EditorGUI.Popup(new Rect(rect.x + 25f, rect.y, rect.width - 300f, rect.height), _materiaSetter.MateriaCategory[element.intValue], category, EditorStyles.popup);

                //_colorData = _colorSetter.ColorData[element.intValue];
                Materia oldCD = _materiaSetter.Materia[element.intValue];
                _materiaSetter.Materia[element.intValue] = (Materia)EditorGUI.ObjectField(new Rect(rect.x + 170f, rect.y, rect.width - 195f, rect.height), _materiaSetter.Materia[element.intValue], typeof(Materia), false);
                if (EditorGUI.EndChangeCheck())
                {
                    if (_materiaSetter.Materia[element.intValue] == null)
                        _materiaSetter.Materia[element.intValue] = Utils.Settings.DefaultMateria;

                    serializedObject.Update();

                    if (_materiaSetter.Materia[element.intValue] != oldCD)
                        //_isColorSetterDirty.boolValue = true;

                    serializedObject.ApplyModifiedProperties();
                    //_emissionInUse = IsEmissionInUse(_materiaSetter.Materia);
                }

                Rect cdExpandRect = new Rect(EditorGUIUtility.currentViewWidth - 60f, rect.y, 20f, 20f);
                if (GUI.Button(cdExpandRect, new GUIContent(EditorGUIUtility.IconContent("d_editicon.sml").image, "Edit Color Data")))
                    EditorUtils.InspectTarget(_materiaSetter.Materia[element.intValue]);

                // This can be optimized
                if (!_materiaReorderableList.HasKeyboardControl()) Reload();
                // -------------
            };

            _materiaReorderableList.onSelectCallback = (ReorderableList list) =>
            {
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(list.index);
                if (!_materiaSetter.Materia.ContainsKey(element.intValue)) return;

                Reload();

                //HighlightSelectedColorData(list.index);
            };

            _materiaReorderableList.onMouseUpCallback = (ReorderableList list) =>
            {
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(list.index);
                if (!_materiaSetter.Materia.ContainsKey(element.intValue)) return;

                if (Utils.Settings.HighlightMode == HighlightMode.WhileLMBHeld) Reload();
            };

            /*_colorDataList.onReorderCallback = (ReorderableList list) =>
            {
                if (_settings.HighlightMode == HighlightMode.WhileLMBHeld)
                    _colorDataList.ReleaseKeyboardFocus();

                Refresh();
                _isColorSetterDirty.boolValue = true;
            };*/
        }

        private void GenerateMateriaDictionary(bool resetToDefault = false)
        {
            if (!_materiaSetter.IsInitialized || _materiaSetter.Materia.Count == 0)
            {
                var newMateriaDictionary = new SerializableIntMateriaDictionary();

                var rects = MeshAnalyzer.CalculateRects(Utils.Settings.GridSize);
                _materiaSetter.FilteredRects = MeshAnalyzer.FilterRects(rects, _materiaSetter.Mesh.uv);

                foreach (var rect in _materiaSetter.FilteredRects)
                {
                    newMateriaDictionary.Add(rect.Key, Utils.Settings.DefaultMateria);
                }

                _materiaSetter.Materia = newMateriaDictionary;
                _materiaSetter.Rects = rects;
            }
        }

        private void Reload()
        {
            Initialize();
        }

        /*private bool CheckAllSystems()
        {
            if (MateriatorSettings.Instance == null)
            {
                return ErrorMessage("Please first go to Tools->Materiator->Settings to generate a settings file.", true);
            }
            if ((_materiaSetter.MeshRenderer != null && _materiaSetter.SkinnedMeshRenderer != null) || (_materiaSetter.MeshFilter != null && _materiaSetter.SkinnedMeshRenderer != null))
            {
                return ErrorMessage("Please use either only a SKINNED MESH RENDERER component alone or a MESH FILTER + MESH RENDERER component combo.");
            }
            else if (_materiaSetter.SkinnedMeshRenderer == null && _materiaSetter.MeshFilter == null)
            {
                return ErrorMessage("Please first add a MESH FILTER component to this Game Object.");
            }
            else if (_materiaSetter.SkinnedMeshRenderer == null && _materiaSetter.MeshFilter.sharedMesh == null)
            {
                return ErrorMessage("Please first add a MESH first and hit the Retry button.", true);
            }
            else if (_materiaSetter.SkinnedMeshRenderer == null && _materiaSetter.MeshRenderer == null)
            {
                return ErrorMessage("Please first add a MESH RENDERER or a SKINNED MESH RENDERER component to this Game Object.");
            }
            else
            {
                return true;
            }
        }

        private bool ErrorMessage(string text, bool retryButton = false)
        {
            root = new VisualElement();
            tree = CreateInstance<VisualTreeAsset>();
            var warning = new HelpBox(text, HelpBoxMessageType.Warning);
            root.Add(warning);

            if (retryButton)
            {
                var button = new Button(Reload);
                button.text = "Retry";
                root.Add(button);
            }

            return false;
        }*/

        private void SetUpButtons()
        {
            _reloadButton = root.Q<Button>("ReloadButton");
            _reloadButton.clicked += Reload;
        }
    }
}
