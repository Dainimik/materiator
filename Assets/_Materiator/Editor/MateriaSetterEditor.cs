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


            _materiaReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("MateriaSlots"), false, true, false, false);
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

            //GenerateMateriaDictionary();
            GenerateMateriaSlots();

            _materiaSetter.UpdateTexturePixelColors();
        }

        private void MateriaReorderableList()
        {
            _materiaReorderableList.DoLayoutList();
        }

        private void ReloadList()
        {
            category = Utils.MateriaTags.MateriaTagDictionary.Values.ToArray();
        }

        private void DrawMateriaReorderableList()
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
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(index);
                var elementID = element.FindPropertyRelative("ID");
                var elementMateria = element.FindPropertyRelative("Materia").objectReferenceValue as Materia;
                var materiaTag = element.FindPropertyRelative("MateriaTag");

                Rect r = new Rect(rect.x, rect.y, 22f, 22f);

                var color = elementMateria.BaseColor;
                var emission = elementMateria.EmissionColor;

                color.a = 255;
                emission.a = 1f;

                if (elementMateria.IsEmissive)
                {
                    var texE = new Texture2D(4, 4);
                    Rect texRE = new Rect(rect.x + 45f, rect.y, 20f, 20f);
                    GUI.DrawTexture(texRE, texE, ScaleMode.StretchToFill, false, 0, emission, 0, 0);
                }

                var tex = new Texture2D(4, 4);
                //Rect r = new Rect(rect.x, rect.y, 20f, 20f);
                //GUI.DrawTexture(r, tex, ScaleMode.StretchToFill, false, 0, color, 0, 0);

                EditorGUI.LabelField(r, new GUIContent((elementID.intValue + 1).ToString()));
                //EditorGUI.LabelField(new Rect(rect.x + 20f, rect.y, rect.width, rect.height), new GUIContent(_materiaSetter.Materia[element.intValue].PreviewIconGray));

                EditorGUI.BeginChangeCheck();
                //EditorGUI.PropertyField(new Rect(rect.x + 50f, rect.y, rect.width - 75f, rect.height), element, GUIContent.none);

                serializedObject.Update();
                materiaTag.intValue = EditorGUI.Popup(new Rect(rect.x + 25f, rect.y, rect.width - 300f, rect.height), materiaTag.intValue, category, EditorStyles.popup);
                if (EditorGUI.EndChangeCheck())
                {
                    var canSetTag = true;
                    for (int i = 0; i < _materiaSetter.MateriaSlots.Count; i++)
                    {
                        if (_materiaSetter.MateriaSlots[i].MateriaTag == materiaTag.intValue)
                        {
                            canSetTag = false;
                        }
                    }

                    if (canSetTag || materiaTag.intValue == 0)
                    {
                        Undo.RegisterCompleteObjectUndo(_materiaSetter, "Change Materia Tag");
                        _materiaSetter.MateriaSlots[index].MateriaTag = materiaTag.intValue;
                    }
                }

                //_colorData = _colorSetter.ColorData[element.intValue];
                Materia oldCD = elementMateria;
                elementMateria = (Materia)EditorGUI.ObjectField(new Rect(rect.x + 170f, rect.y, rect.width - 195f, rect.height), elementMateria, typeof(Materia), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(_materiaSetter, "Change Materia");
                    if (elementMateria == null)
                        elementMateria = Utils.Settings.DefaultMateria;
                    else
                        _materiaSetter.MateriaSlots[index].Materia = elementMateria;

                    serializedObject.Update();

                    if (elementMateria != oldCD)
                        //_isColorSetterDirty.boolValue = true;

                    serializedObject.ApplyModifiedProperties();
                    //_emissionInUse = IsEmissionInUse(_materiaSetter.Materia);
                }

                Rect cdExpandRect = new Rect(EditorGUIUtility.currentViewWidth - 60f, rect.y, 20f, 20f);
                if (GUI.Button(cdExpandRect, new GUIContent(EditorGUIUtility.IconContent("d_editicon.sml").image, "Edit Color Data")))
                    EditorUtils.InspectTarget(elementMateria);

                // This can be optimized
                if (!_materiaReorderableList.HasKeyboardControl()) Reload();
                // -------------
            };

            _materiaReorderableList.onSelectCallback = (ReorderableList list) =>
            {
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(list.index);

                Reload();

                //HighlightSelectedColorData(list.index);
            };

            _materiaReorderableList.onMouseUpCallback = (ReorderableList list) =>
            {
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(list.index);

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

        /*private void GenerateMateriaDictionary(bool resetToDefault = false)
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
        }*/

        private void GenerateMateriaSlots()
        {
            if (!_materiaSetter.IsInitialized || _materiaSetter.MateriaSlots.Count == 0)
            {
                var rects = MeshAnalyzer.CalculateRects(Utils.Settings.GridSize);
                _materiaSetter.FilteredRects = MeshAnalyzer.FilterRects(rects, _materiaSetter.Mesh.uv);
                _materiaSetter.MateriaSlots = new System.Collections.Generic.List<MateriaSlot>();

                foreach (var rect in _materiaSetter.FilteredRects)
                {
                    _materiaSetter.MateriaSlots.Add(new MateriaSlot(rect.Key));
                }

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
