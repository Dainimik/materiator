using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.UIElements;
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

        #region Serialized Properties

        private SerializedProperty _materiaPreset;
        private SerializedProperty _materiaSetterData;
        private SerializedProperty _shaderData;
        private SerializedProperty _material;

        #endregion

        private ObjectField _materiaPresetObjectField;
        private ObjectField _materiaSetterDataObjectField;
        private ObjectField _shaderDataObjectField;

        private Button _reloadButton;
        private Button _newMateriaPresetButton;
        private Button _cloneMateriaPresetButton;
        private Button _reloadMateriaPresetButton;
        private Button _overwriteMateriaSetterData;
        private Button _saveAsNewMateriaSetterData;

        private void OnEnable()
        {
            _materiaSetter = (MateriaSetter)target;

            Initialize();
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaSetter>();

            DrawPresetSection();
            DrawOutputSection();
            
            DrawIMGUI();

            return root;
        }

        private void DrawIMGUI()
        {
            IMGUIContainer defaultInspector = new IMGUIContainer(() => DrawDefaultInspector());
            root.Add(defaultInspector);

            _materiaReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("MateriaSlots"), false, true, false, false);
            DrawMateriaReorderableList();
            IMGUIContainer materiaReorderableList = new IMGUIContainer(() => MateriaReorderableList());
            root.Add(materiaReorderableList);
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
            //_materiaTagArray = Utils. ReloadTagList();
        }

        private void DrawPresetSection()
        {
            if (_materiaSetter.MateriaPreset == null)
            {
                _reloadMateriaPresetButton.visible = true;
            }
            else
            {
                _reloadMateriaPresetButton.visible = false;
            }

            _materiaPresetObjectField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                LoadPreset((MateriaPreset)_materiaPresetObjectField.value);
            });
        }

        private void DrawOutputSection()
        {

        }

        private void LoadPreset(MateriaPreset preset)
        {
            if (preset != null)
            {
                _reloadMateriaPresetButton.visible = true;
                _materiaSetter.LoadPreset(preset);
            }
            else
            {
                _reloadMateriaPresetButton.visible = false;
            }
            serializedObject.Update();
        }

        private void MateriaReorderableList()
        {
            _materiaReorderableList.DoLayoutList();
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
                //var materiaTagID = element.FindPropertyRelative("MateriaTag").FindPropertyRelative("ID");
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

                int _materiaTagIndex = 0;
                _materiaTagIndex = EditorGUI.Popup(new Rect(rect.x + 25f, rect.y, 95f, rect.height), Utils.MateriaTags.MateriaTagsList.IndexOf(materiaTag.stringValue), Utils.MateriaTags.MateriaTagsArray, EditorStyles.popup);
                if (EditorGUI.EndChangeCheck())
                {
                    var newTag = Utils.MateriaTags.MateriaTagsList[_materiaTagIndex];
                    Undo.RegisterCompleteObjectUndo(_materiaSetter, "Change Materia Tag");
                    _materiaSetter.MateriaSlots[index].MateriaTag = newTag;
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
            if (!_materiaSetter.IsInitialized || _materiaSetter.MateriaSlots?.Count == 0)
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

        private void NewPreset()
        {
            _materiaPreset.objectReferenceValue = null;

            CreateEditorMaterial(false, _materiaSetter.gameObject.name);
            //GenerateColorDataArray(true);
            //EditorUtility.SetDirty(_materiaSetter);

            serializedObject.ApplyModifiedProperties();
        }

        private void ClonePreset()
        {

        }

        private void ReloadPreset()
        {

        }

        private void OverwriteMateriaSetterData()
        {
            WriteAssetsToDisk(AssetDatabase.GetAssetPath(_materiaSetter.MateriaSetterData), false, Utils.Settings.PackAssets);
            _materiaSetter.UpdateRenderer(false);
        }
        private void SaveAsNewMateriaSetterData()
        {
            WriteAssetsToDisk(null, true, Utils.Settings.PackAssets);
            _materiaSetter.UpdateRenderer(false);
        }

        public void WriteAssetsToDisk(string path, bool saveAsNew, bool packAssets)
        {
            string name;
            string dir;

            if (path != null)
            {
                dir = AssetUtils.GetDirectoryName(path);
                name = AssetUtils.GetFileName(path);
            }
            else
            {
                dir = Utils.Settings.SavePath;
                name = _materiaSetter.gameObject.name;
                path = dir + name + ".asset";
            }

            var shaderData = (ShaderData)_shaderDataObjectField.value;
            Material mat;
            Textures texs;
            var matName = name + "_Material";

            bool wasMPExisting;
            var data = AssetUtils.CreateOrReplaceScriptableObjectAsset(_materiaSetter.MateriaSetterData, path, out wasMPExisting);

            if (wasMPExisting)
                texs = data.Textures;
            else
                texs = _materiaSetter.Textures;

            var presetFolderDir = dir;

            var allAssetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
            if (saveAsNew && allAssetsAtPath.Length < 2)
            {
                mat = Instantiate(_materiaSetter.Material);
                if (packAssets)
                {
                    mat.name = matName;
                    texs = texs.CopyTextures(texs, Utils.Settings.FilterMode);
                    texs.SetNames(name);

                    AssetDatabase.AddObjectToAsset(mat, data);
                    texs.AddTexturesToAsset(data);
                }
                else
                {
                    AssetDatabase.CreateAsset(mat, presetFolderDir + matName + ".mat");
                    mat = (Material)AssetDatabase.LoadAssetAtPath(presetFolderDir + matName + ".mat", typeof(Material));

                    texs.SetNames(name);
                    texs.WriteTexturesToDisk(presetFolderDir);
                }

                _materiaSetter.Material = mat;
                _materiaSetter.Textures = texs;
                data.Material = mat;
                data.Textures = texs;
            }
            else
            {
                mat = data.Material;
                if (packAssets)
                {
                    data.Textures.CopySerialized(texs);
                    data.Textures.SetTextures(data.Material, (ShaderData)_shaderData.objectReferenceValue);
                }
                else
                {
                    if (mat == null)
                    {
                        AssetDatabase.CreateAsset(mat, presetFolderDir + "/" + matName + ".mat");
                        mat = (Material)AssetDatabase.LoadAssetAtPath(presetFolderDir + "/" + matName + ".mat", typeof(Material));
                    }

                    data.Textures.WriteTexturesToDisk(presetFolderDir);
                    data.Textures.SetTextures(data.Material, (ShaderData)_shaderData.objectReferenceValue);
                }
            }

            _materiaSetter.SetTextures();
            _materiaSetter.MateriaSetterData = data;

            serializedObject.Update();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtils.MarkOpenPrefabSceneDirty();
        }

        private void CreateEditorMaterial(bool clone = false, string name = null)
        {
            Utils.CreateMaterial(Utils.Settings.DefaultShaderData.Shader, name);

            serializedObject.Update();
            var newTextures = new Textures();

            if (!clone)
            {
                _material.objectReferenceValue = Utils.CreateMaterial(_materiaSetter.ShaderData.Shader, name);
                newTextures.CreateTextures(Utils.Settings.GridSize, Utils.Settings.GridSize);
            }
            else
            {
                _material.objectReferenceValue = Instantiate(_material.objectReferenceValue);
                newTextures = newTextures.CopyTextures(_materiaSetter.Textures, Utils.Settings.FilterMode);
            }

            if (name != null)
                _material.objectReferenceValue.name = name;

            serializedObject.ApplyModifiedProperties();

            _materiaSetter.Textures = newTextures;
            _materiaSetter.SetTextures();
            serializedObject.Update();
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
        protected override void GetProperties()
        {
            _materiaPreset = serializedObject.FindProperty("MateriaPreset");
            _materiaSetterData = serializedObject.FindProperty("MateriaSetterData");
            _shaderData = serializedObject.FindProperty("ShaderData");
            _material = serializedObject.FindProperty("Material");

            _materiaPresetObjectField = root.Q<ObjectField>("MateriaPresetObjectField");
            _materiaPresetObjectField.objectType = typeof(MateriaPreset);
            _materiaSetterDataObjectField = root.Q<ObjectField>("MateriaSetterDataObjectField");
            _materiaSetterDataObjectField.objectType = typeof(MateriaSetterData);
            _shaderDataObjectField = root.Q<ObjectField>("ShaderDataObjectField");
            _shaderDataObjectField.objectType = typeof(ShaderData);


            _reloadButton = root.Q<Button>("ReloadButton");
            _newMateriaPresetButton = root.Q<Button>("NewMateriaPresetButton");
            _cloneMateriaPresetButton = root.Q<Button>("CloneMateriaPresetButton");
            _reloadMateriaPresetButton = root.Q<Button>("ReloadMateriaPresetButton");
            _overwriteMateriaSetterData = root.Q<Button>("OverwriteMateriaSetterDataButton");
            _saveAsNewMateriaSetterData = root.Q<Button>("SaveAsNewMateriaSetterDataButton");
        }

        protected override void BindProperties()
        {
            _materiaPresetObjectField.BindProperty(_materiaPreset);
            _materiaSetterDataObjectField.BindProperty(_materiaSetterData);
            _shaderDataObjectField.BindProperty(_shaderData);
        }

        protected override void RegisterButtons()
        {
            _reloadButton.clicked += Reload;
            _newMateriaPresetButton.clicked += NewPreset;
            _cloneMateriaPresetButton.clicked += ClonePreset;
            _reloadMateriaPresetButton.clicked += ReloadPreset;
            _overwriteMateriaSetterData.clicked += OverwriteMateriaSetterData;
            _saveAsNewMateriaSetterData.clicked += SaveAsNewMateriaSetterData;
        }
    }
}
