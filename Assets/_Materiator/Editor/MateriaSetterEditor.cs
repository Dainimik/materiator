using System.Linq;
using System.Reflection;
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

        private SerializedProperty _isDirty;
        private SerializedProperty _materiaPreset;
        private SerializedProperty _materiaSetterData;
        private SerializedProperty _shaderData;
        private SerializedProperty _material;

        #endregion

        private ObjectField _materiaPresetObjectField;
        private ObjectField _materiaSetterDataObjectField;
        private ObjectField _shaderDataObjectField;

        private Button _reloadButton;
        private Button _reloadMateriaPresetButton;
        private Button _newMateriaSetterDataButton;
        private Button _cloneMateriaSetterDataButton;
        private Button _reloadMateriaSetterDataButton;
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
            DrawDataSection();
            DrawOutputSection();
            
            DrawIMGUI();

            return root;
        }

        private void SetMateriaSetterDirty(bool value)
        {
            if (value)
            {
                _isDirty.boolValue = true;
            }
            else
            {
                _isDirty.boolValue = false;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DiscardChanges()
        {
            FieldInfo[] fields = typeof(MateriaSetter).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var item in fields)
            {
                Debug.Log(item);
            }
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
        }

        private void ResetMateriaSetter()
        {
            //_materiaSetter.ResetMateriaSetter();

            SetMateriaSetterDirty(true);
        }

        private void DrawPresetSection()
        {
            if (_materiaPresetObjectField.value == null)
            {
                _reloadMateriaPresetButton.visible = false;
            }
            else
            {
                _reloadMateriaPresetButton.visible = true;
            }

            _materiaPresetObjectField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                OnMateriaPresetChanged();
            });
        }

        private void DrawDataSection()
        {
            if (_materiaSetterDataObjectField.value == null)
            {
                _reloadMateriaSetterDataButton.visible = false;
            }
            else
            {
                _reloadMateriaSetterDataButton.visible = true;
            }

            _materiaSetterDataObjectField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                OnMateriaSetterDataChanged();
            });

            _shaderDataObjectField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                OnShaderDataChanged();
            });
        }

        private void DrawOutputSection()
        {

        }

        private void LoadPreset(MateriaPreset preset)
        {
            var numberOfSameMateria = 0;

            if (preset != null)
            {
                _reloadMateriaPresetButton.visible = true;
                _materiaSetter.LoadPreset(preset, out numberOfSameMateria);
            }
            else
            {
                _reloadMateriaPresetButton.visible = false;
                _materiaSetter.LoadPreset(null, out numberOfSameMateria);
            }
            serializedObject.Update();

            if (numberOfSameMateria == _materiaSetter.MateriaSetterData.MateriaSlots.Count)
            {
                SetMateriaSetterDirty(false);
            }
            else
            {
                SetMateriaSetterDirty(true);
            }
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
                    _materiaSetter.UpdateColorsOfAllTextures();

                    if (elementMateria != oldCD)
                        //_isColorSetterDirty.boolValue = true;

                    serializedObject.ApplyModifiedProperties();
                    //_emissionInUse = IsEmissionInUse(_materiaSetter.Materia);
                }

                Rect cdExpandRect = new Rect(EditorGUIUtility.currentViewWidth - 60f, rect.y, 20f, 20f);
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

        Texture2D _highlightedTexture;
        private void HandleMateriaSlotSelection(int index, bool selected)
        {
            var originalTexture = _materiaSetter.Textures.Color;

            if (_highlightedTexture == null)
            {
                _highlightedTexture = new Texture2D(_materiaSetter.Textures.Color.width, _materiaSetter.Textures.Color.height, TextureFormat.RGBA32, false);
                EditorUtility.CopySerialized(originalTexture, _highlightedTexture);
                _highlightedTexture.filterMode = Utils.Settings.FilterMode;
            }

            var colors = originalTexture.GetPixels32();
            var rectInt = Utils.GetRectIntFromRect(Utils.Settings.GridSize, _materiaSetter.FilteredRects[index]);

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Utils.Settings.HighlightColor;
            }

            if (selected)
            {
                _highlightedTexture.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, colors);
                _highlightedTexture.Apply();

                _materiaSetter.Renderer.sharedMaterial.SetTexture(_materiaSetter.ShaderData.MainTexturePropertyName, _highlightedTexture);
            }
            else
            {
                _highlightedTexture.SetPixels32(originalTexture.GetPixels32());
                _highlightedTexture.Apply();

                _materiaSetter.Renderer.sharedMaterial.SetTexture(_materiaSetter.ShaderData.MainTexturePropertyName, originalTexture);
            }
        }

        private void Refresh()
        {
            _materiaSetter.Refresh();
        }

        private void ReloadPreset()
        {
            LoadPreset((MateriaPreset)_materiaPresetObjectField.value);
        }

        private void NewData()
        {
            ResetMateriaSetter();
        }

        private void CloneData()
        {

        }

        private void ReloadData()
        {
            OnMateriaSetterDataChanged();
        }

        private void OnMateriaPresetChanged()
        {
            LoadPreset((MateriaPreset)_materiaPresetObjectField.value);
        }

        private void OnMateriaSetterDataChanged()
        {
            if (_materiaSetterDataObjectField.value != null)
            {
                //_reloadMateriaSetterDataButton.visible = true;

                _materiaSetterData.objectReferenceValue = _materiaSetterDataObjectField.value;
                var data = (MateriaSetterData)_materiaSetterDataObjectField.value;

                serializedObject.ApplyModifiedProperties();

                Utils.CopyFields(data, _materiaSetter);

                serializedObject.Update();

                _materiaSetter.UpdateRenderer();
            }
            else
            {
                _reloadMateriaSetterDataButton.visible = false;

                _materiaSetterDataObjectField.value = _materiaSetterData.objectReferenceValue;
            }

            SetMateriaSetterDirty(false);
        }

        private void OnShaderDataChanged()
        {
            SetMateriaSetterDirty(true);

            _materiaSetter.Renderer.sharedMaterial.shader = ((ShaderData)_shaderDataObjectField.value).Shader;

            serializedObject.ApplyModifiedProperties();
        }

        private void OverwriteMateriaSetterData()
        {
            if (EditorUtility.DisplayDialog("Overwrite current data?", "Are you sure you want to overwrite " + _materiaSetterData.objectReferenceValue.name + " with current settings?", "Yes", "No"))
            {
                WriteAssetsToDisk(AssetDatabase.GetAssetPath(_materiaSetter.MateriaSetterData), Utils.Settings.PackAssets);
                _materiaSetter.UpdateRenderer(false);
            }
        }
        private void SaveAsNewMateriaSetterData()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save data", _materiaSetter.gameObject.name, "asset", "asset");
            if (path.Length != 0)
            {
                WriteAssetsToDisk(path, Utils.Settings.PackAssets);
                _materiaSetter.UpdateRenderer(false);
            }    
        }

        public void WriteAssetsToDisk(string path, bool packAssets)
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

            Textures texs = new Textures();

            bool wasMPExisting;
            var data = AssetUtils.CreateOrReplaceScriptableObjectAsset(_materiaSetter.MateriaSetterData, path, out wasMPExisting);

            if (wasMPExisting)
                texs = data.Textures;
            else
                texs.CreateTextures((int)_materiaSetter.Textures.Size.x, (int)_materiaSetter.Textures.Size.y);
                //texs = _materiaSetter.Textures;






            var mat = Instantiate(_materiaSetter.Material);

            texs.CopyTextures(_materiaSetter.Textures, Utils.Settings.FilterMode);
            texs.SetNames(name);

            if (packAssets)
            {
                mat.name = name + "_Material";

                AssetDatabase.AddObjectToAsset(mat, data);
                texs.AddTexturesToAsset(data);
            }
            else
            {
                AssetUtils.CheckDirAndCreate(dir, name);
                var folderDir = dir + "/" + name + "/";

                AssetDatabase.CreateAsset(mat, folderDir + name + "_Material.mat");
                mat = (Material)AssetDatabase.LoadAssetAtPath(folderDir + name + "_Material.mat", typeof(Material));

                texs.WriteTexturesToDisk(folderDir);
            }

            _materiaSetter.Material = mat;
            _materiaSetter.Textures = texs;

            data.MateriaSlots = _materiaSetter.MateriaSlots;
            data.ShaderData = (ShaderData)_shaderData.objectReferenceValue;
            data.MateriaPreset = (MateriaPreset)_materiaPreset.objectReferenceValue;
            data.Material = mat;
            data.Textures = texs;






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
                newTextures.CopyTextures(_materiaSetter.Textures, Utils.Settings.FilterMode);
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
            _isDirty = serializedObject.FindProperty("IsDirty");
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
            _reloadMateriaPresetButton = root.Q<Button>("ReloadMateriaPresetButton");
            _newMateriaSetterDataButton = root.Q<Button>("NewMateriaSetterDataButton");
            _cloneMateriaSetterDataButton = root.Q<Button>("CloneMateriaSetterDataButton");
            _reloadMateriaSetterDataButton = root.Q<Button>("ReloadMateriaSetterDataButton");
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
            _reloadButton.clicked += Refresh;
            _reloadMateriaPresetButton.clicked += ReloadPreset;
            _newMateriaSetterDataButton.clicked += NewData;
            _cloneMateriaSetterDataButton.clicked += CloneData;
            _reloadMateriaSetterDataButton.clicked += ReloadData;
            _overwriteMateriaSetterData.clicked += OverwriteMateriaSetterData;
            _saveAsNewMateriaSetterData.clicked += SaveAsNewMateriaSetterData;
        }
    }
}
