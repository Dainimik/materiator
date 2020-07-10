using System.Collections.Generic;
using System.Linq;
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

        private SerializedProperty _editMode;
        private SerializedProperty _isDirty;
        private SerializedProperty _materiaAtlas;
        private SerializedProperty _materiaPreset;
        private SerializedProperty _materiaSetterData;
        private SerializedProperty _materialData;
        private SerializedProperty _material;

        #endregion

        private ObjectField _materiaAtlasObjectField;
        private ObjectField _materiaPresetObjectField;
        private ObjectField _materiaSetterDataObjectField;
        private ObjectField _materialDataObjectField;

        private Button _reloadButton;
        private Button _switchEditModeButton;
        private Button _reloadMateriaPresetButton;
        private Button _newMateriaSetterDataButton;
        private Button _reloadMateriaSetterDataButton;
        private Button _overwriteMateriaSetterData;
        private Button _saveAsNewMateriaSetterData;

        private VisualElement _atlasIndicator;
        private VisualElement _presetIndicator;
        private VisualElement _outputIndicator;
        private VisualElement _dataIndicator;

        private VisualElement _IMGUIContainer;

        private VisualElement _uvIslandContainer;
        private EnumField _uvDisplayModeEnumField;

        private Label _currentShaderLabel;

        private void OnEnable()
        {
            _materiaSetter = (MateriaSetter)target;

            Initialize();
        }

        private void OnDisable()
        {
            _materiaSetter = (MateriaSetter)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaSetter>();

            DrawAtlasSection();
            DrawPresetSection();
            DrawDataSection();
            DrawOutputSection();

            DrawIMGUI();

            return root;
        }

        private void Initialize()
        {
            _materiaSetter.Initialize();

            /*if (!CheckAllSystems())
            {
                return;
            }*/

            /*if (!_materiaSetter.IsDirty && _materiaSetter.MateriaSetterData != null)
            {
                ReloadData();
            }*/
        }

        private void SetMateriaSetterDirty(bool value)
        {
            serializedObject.Update();
            if (value)
            {
                if (!_isDirty.boolValue)
                {
                    _isDirty.boolValue = true;

                    CreateEditModeData(_editMode.enumValueIndex);
                }
            }
            else
            {
                if (_isDirty.boolValue)
                {
                    _isDirty.boolValue = false;
                }  
            }

            DrawUVInspector(true);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawIMGUI()
        {
            IMGUIContainer defaultInspector = new IMGUIContainer(() => IMGUI());
            root.Add(defaultInspector);

            _materiaReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("MateriaSlots"), true, true, false, false);
            DrawMateriaReorderableList();
            IMGUIContainer materiaReorderableList = new IMGUIContainer(() => MateriaReorderableList());
            _IMGUIContainer.Add(materiaReorderableList);
        }

        private void IMGUI()
        {
            if (_isDirty.boolValue == true)
            {
                _outputIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
                _dataIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
            }
            else
            {
                _outputIndicator.style.backgroundColor = SystemData.Settings.GUIGreen;
                _dataIndicator.style.backgroundColor = SystemData.Settings.GUIGreen;
            }

            DrawDefaultInspector();
        }

        private void ResetMateriaSetter()
        {
            _materiaSetter.ResetMateriaSetter();

            SetMateriaSetterDirty(true);
        }

        private void DrawAtlasSection()
        {
            OnMateriaAtlasChanged();

            _materiaAtlasObjectField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                OnMateriaAtlasChanged();
            });
        }

        private void DrawPresetSection()
        {
            OnMateriaPresetChanged();

            _materiaPresetObjectField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                OnMateriaPresetChanged();
            });
        }

        private void DrawDataSection()
        {
            DrawUVInspector(false);

            _currentShaderLabel.text = _materiaSetter.MaterialData.ShaderData.Shader.name;

            if (_materiaSetterDataObjectField.value == null)
            {
                _reloadMateriaSetterDataButton.SetEnabled(false);
            }
            else
            {
                _reloadMateriaSetterDataButton.SetEnabled(true);
            }

            _materiaSetterDataObjectField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                OnMateriaSetterDataChanged((MateriaSetterData)e.newValue);
            });

            _uvDisplayModeEnumField.RegisterCallback<ChangeEvent<System.Enum>>(e =>
            {
                _uvDisplayModeEnumField.value = e.newValue;
                DrawUVInspector(true);
            });

            _materialDataObjectField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                OnMaterialDataChanged((MaterialData)e.newValue);
            });
        }

        public enum UVDisplayMode
        {
            BaseColor,
            MetallicSpecularGlossSmoothness,
            EmissionColor
        }

        private UVDisplayMode _uvDisplayMode;

        private void DrawUVInspector(bool redraw)
        {
            if (redraw)
            {
                _uvIslandContainer.Clear();
            }

            var rects = _materiaSetter.Rects;
            var size = Mathf.Sqrt(rects.Length);
            Color borderColor;

            for (int i = 0, y = 0; y < size; y++)
            {
                var horizontalContainer = new VisualElement();
                horizontalContainer.style.flexGrow = 1;
                horizontalContainer.style.flexShrink = 0;
                horizontalContainer.style.flexDirection = FlexDirection.Row;
                _uvIslandContainer.Add(horizontalContainer);

                for (int x = 0; x < size; x++, i++)
                {
                    var item = new VisualElement();
                    item.name = "UVGridItem";
                    item.styleSheets.Add(Resources.Load<StyleSheet>("Materiator"));

                    if (_materiaSetter.FilteredRects.ContainsKey(i))
                    {
                        Color color = SystemData.Settings.DefaultMateria.BaseColor;
                        switch (_uvDisplayModeEnumField.value)
                        {
                            case UVDisplayMode.BaseColor:
                                color = _materiaSetter.MateriaSlots.Where(ms => ms.ID == i).First().Materia.BaseColor;
                                break;
                            case UVDisplayMode.MetallicSpecularGlossSmoothness:
                                var metallic = _materiaSetter.MateriaSlots.Where(ms => ms.ID == i).First().Materia.Metallic;
                                var metallicColor = new Color(metallic, metallic, metallic, 1f);
                                color = metallicColor;
                                break;
                            case UVDisplayMode.EmissionColor:
                                color = _materiaSetter.MateriaSlots.Where(ms => ms.ID == i).First().Materia.EmissionColor;
                                break;
                        }

                        borderColor = Color.green;
                        item.style.backgroundColor = color;
                        item.style.borderTopColor = borderColor;
                        item.style.borderBottomColor = borderColor;
                        item.style.borderLeftColor = borderColor;
                        item.style.borderRightColor = borderColor;
                    }
                    else
                    {
                        borderColor = Color.red;
                        item.style.borderTopColor = borderColor;
                        item.style.borderBottomColor = borderColor;
                        item.style.borderLeftColor = borderColor;
                        item.style.borderRightColor = borderColor;
                    }

                    horizontalContainer.Add(item);

                    var label = new Label();
                    label.text = (i + 1).ToString();
                    item.Add(label);
                }
            }
        }

        private void DrawOutputSection()
        {
            if (_materiaSetterData.objectReferenceValue == null)
            {
                _overwriteMateriaSetterData.SetEnabled(false);
            }
            else
            {
                _overwriteMateriaSetterData.SetEnabled(true);
            }
        }

        private void LoadAtlas(MateriaAtlas atlas)
        {
            if (atlas != null)
            {
                _materiaSetter.MateriaSlots = _materiaSetter.MateriaSetterData.MateriaSlots;

                _materiaSetter.LoadAtlas(atlas);
            }

            SetMateriaSetterDirty(false);
        }

        private void UnloadAtlas()
        {
            _materiaSetter.UnloadAtlas();
        }

        private void LoadPreset(MateriaPreset preset)
        {
            List<MateriaSlot> materiaSlots;
            if (_materiaSetter.MateriaSetterData != null)
            {
                materiaSlots = _materiaSetter.MateriaSetterData.MateriaSlots;
            }
            else
            {
                materiaSlots = _materiaSetter.MateriaSlots;
            }
            var same = AreMateriasSameAsPreset(preset, _materiaSetter.MateriaSlots);

            if (!same)
            {
                SetMateriaSetterDirty(true);

                if (preset != null)
                {
                    _reloadMateriaPresetButton.visible = true;
                    _materiaSetter.LoadPreset(preset);
                }
                else
                {
                    _reloadMateriaPresetButton.visible = false;
                    _materiaSetter.LoadPreset(null);
                }
                serializedObject.Update();

                _materiaSetter.UpdateColorsOfAllTextures();
            }
            else
            {
                SetMateriaSetterDirty(false);
            }

            DrawUVInspector(true);
        }

        private void MateriaReorderableList()
        {
            _materiaReorderableList.DoLayoutList();
        }

        private void CreateEditModeData(int editMode)
        {
            if (_materiaSetterData.objectReferenceValue != null)
            {
                var newTextures = new Textures();

                Textures textures = null;

                if (editMode == 0)
                {
                    textures = _materiaSetter.MateriaSetterData.Textures;
                }
                else if (editMode == 1)
                {
                    textures = _materiaSetter.MateriaSetterData.MateriaAtlas.Textures;
                }

                newTextures.CreateTextures(textures.Size.x, textures.Size.y);
                var mat = Instantiate(_material.objectReferenceValue);
                newTextures.CopyPixelColors(textures, textures.Size.x, new Rect(0, 0, 1, 1), newTextures.Size.x, new Rect(0, 0, 1, 1));          

                if (name != null)
                    mat.name = name;

                _material.objectReferenceValue = mat;

                serializedObject.ApplyModifiedProperties();

                _materiaSetter.Textures = newTextures;
                _materiaSetter.SetTextures();

                var newMateriaSlots = new List<MateriaSlot>();

                foreach (var item in _materiaSetter.MateriaSetterData.MateriaSlots)
                {
                    newMateriaSlots.Add(new MateriaSlot(item.ID, item.Materia, item.Tag));
                }

                _materiaSetter.MateriaSlots = newMateriaSlots;
                serializedObject.Update();
                _materiaSetter.UpdateRenderer();
                serializedObject.Update();
            }
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
                serializedObject.Update();
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(index);
                var elementID = element.FindPropertyRelative("ID");
                var elementMateria = element.FindPropertyRelative("Materia").objectReferenceValue as Materia;
                var materiaTag = element.FindPropertyRelative("Tag");

                Rect r = new Rect(rect.x, rect.y, 22f, 22f);

                var color = elementMateria.BaseColor;
                var emission = elementMateria.EmissionColor;

                color.a = 255;
                emission.a = 255;

                if (elementMateria.IsEmissive)
                {
                    var texE = new Texture2D(4, 4);
                    Rect texRE = new Rect(rect.x + 45f, rect.y, 20f, 20f);
                    GUI.DrawTexture(texRE, texE, ScaleMode.StretchToFill, false, 0, emission, 0, 0);
                }

                var tex = new Texture2D(4, 4);

                EditorGUI.LabelField(r, new GUIContent((elementID.intValue + 1).ToString()));

                serializedObject.Update();

                int _materiaTagIndex = 0;
                EditorGUI.BeginChangeCheck();
                _materiaTagIndex = EditorGUI.Popup(new Rect(rect.x + 25f, rect.y, 95f, rect.height), SystemData.Settings.MateriaTags.MateriaTagsList.IndexOf(materiaTag.stringValue), SystemData.Settings.MateriaTags.MateriaTagsArray, EditorStyles.popup);
                if (EditorGUI.EndChangeCheck())
                {
                    SetMateriaSetterDirty(true);
                    var newTag = SystemData.Settings.MateriaTags.MateriaTagsList[_materiaTagIndex];
                    Undo.RegisterCompleteObjectUndo(_materiaSetter, "Change Materia Tag");
                    _materiaSetter.MateriaSlots[index].Tag = newTag;
                }

                EditorGUI.BeginChangeCheck();
                elementMateria = (Materia)EditorGUI.ObjectField(new Rect(rect.x + 170f, rect.y, rect.width - 195f, rect.height), elementMateria, typeof(Materia), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(_materiaSetter, "Change Materia");
                    SetMateriaSetterDirty(true);

                    if (elementMateria == null)
                        elementMateria = SystemData.Settings.DefaultMateria;
                    else
                        _materiaSetter.MateriaSlots[index].Materia = elementMateria;

                    serializedObject.Update();
                    DrawUVInspector(true);

                    _materiaSetter.UpdateColorsOfAllTextures();

                    serializedObject.ApplyModifiedProperties();
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

        private void Refresh()
        {
            _materiaSetter.Refresh();
            DrawUVInspector(true);
        }

        private void SwitchEditMode()
        {
            if (_materiaSetter.EditMode == EditMode.Native)
            {
                LoadAtlas(_materiaSetter.MateriaSetterData.MateriaAtlas);
            }
            else if (_materiaSetter.EditMode == EditMode.Atlas)
            {
                UnloadAtlas();
            }

            //SetMateriaSetterDirty(true);
            //_materiaSetter.AnalyzeMesh();
            //_materiaSetter.UpdateColorsOfAllTextures();

            //_materiaSetter.MateriaSlots = _materiaSetter.MateriaSetterData.MateriaSlots;
        }

        private void ReloadPreset()
        {
            LoadPreset((MateriaPreset)_materiaPresetObjectField.value);

            OnMateriaPresetChanged();
        }

        private void NewData()
        {
            ResetMateriaSetter();
        }

        private void ReloadData(MateriaSetterData data)
        {
            OnMateriaSetterDataChanged(data);
        }

        private void OnMateriaAtlasChanged()
        {
            if (_materiaSetter.MateriaSetterData != null)
            {
                if (_materiaSetter.MateriaAtlas != null)
                {
                    _atlasIndicator.style.backgroundColor = SystemData.Settings.GUIGreen;
                }
                else
                {
                    _atlasIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
                }
            }
            else
            {
                _atlasIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
            }

            if (_materiaSetter.MateriaAtlas == null)
            {
                _switchEditModeButton.SetEnabled(false);
            }
            else
            {
                _switchEditModeButton.SetEnabled(true);
            }

            _switchEditModeButton.text = _editMode.enumNames[_editMode.enumValueIndex] + " Mode";
        }

        private void OnMateriaPresetChanged()
        {
            if (_materiaPresetObjectField.value == null)
            {
                _reloadMateriaPresetButton.SetEnabled(false);
                _presetIndicator.style.backgroundColor = SystemData.Settings.GUIGray;
            }
            else
            {
                _reloadMateriaPresetButton.SetEnabled(true);
                
                if (AreMateriasSameAsPreset((MateriaPreset)_materiaPresetObjectField.value, _materiaSetter.MateriaSlots))
                {
                    _presetIndicator.style.backgroundColor = SystemData.Settings.GUIGreen;
                }
                else
                {
                    _presetIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
                }
            }
        }

        private bool AreMateriasSameAsPreset(MateriaPreset preset, List<MateriaSlot> materiaSlots)
        {
            var numberOfDifferentMateria = 0;

            if (preset != null)
            {
                for (int i = 0; i < materiaSlots.Count; i++)
                {
                    for (int j = 0; j < preset.MateriaPresetItemList.Count; j++)
                    {
                        if (materiaSlots[i].Tag == preset.MateriaPresetItemList[j].Tag)
                        {
                            if (materiaSlots[i].Materia != preset.MateriaPresetItemList[j].Materia)
                            {
                                numberOfDifferentMateria++;
                            }
                        }
                    }
                }
            }

            var same = false;

            if (numberOfDifferentMateria == 0)
            {
                same = true;
            }

            return same;
        }

        private void OnMateriaSetterDataChanged(MateriaSetterData data)
        {
            if (_materiaSetterDataObjectField.value != null)
            {
                _materiaSetterData.objectReferenceValue = data;
                serializedObject.ApplyModifiedProperties();

                Utils.ShallowCopyFields(data, _materiaSetter);

                if (_editMode.enumValueIndex == 0)
                {
                    _materiaSetter.Mesh = data.NativeMesh;
                }
                else if (_editMode.enumValueIndex == 1)
                {
                    LoadAtlas(data.MateriaAtlas);
                }

                serializedObject.Update();
                _materiaSetter.UpdateRenderer();
            }
            else
            {
                _reloadMateriaSetterDataButton.SetEnabled(false);

                _materiaSetterDataObjectField.value = _materiaSetterData.objectReferenceValue;
            }

            SetMateriaSetterDirty(false);
        }

        private void OnMaterialDataChanged(MaterialData materialData)
        {
            SetMateriaSetterDirty(true);

            if (materialData.Material.shader != materialData.ShaderData.Shader)
            {
                materialData.Material.shader = materialData.ShaderData.Shader;
            }

            _currentShaderLabel.text = materialData.ShaderData.Shader.name;

            _material.objectReferenceValue = Instantiate(materialData.Material);

            serializedObject.ApplyModifiedProperties();

            _materiaSetter.Material.name = _materiaSetter.gameObject.name;
            _materiaSetter.Material.shader = materialData.ShaderData.Shader;

            _materiaSetter.SetTextures();
            _materiaSetter.UpdateRenderer(false);
        }

        private void OverwriteMateriaSetterData()
        {
            if (EditorUtility.DisplayDialog("Overwrite current data?", "Are you sure you want to overwrite " + _materiaSetterData.objectReferenceValue.name + " with current settings?", "Yes", "No"))
            {
                WriteAssetsToDisk(AssetDatabase.GetAssetPath(_materiaSetter.MateriaSetterData), SystemData.Settings.PackAssets);
                
            }
        }
        private void SaveAsNewMateriaSetterData()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save data", _materiaSetter.gameObject.name, "asset", "asset");
            if (path.Length != 0)
            {
                WriteAssetsToDisk(path, SystemData.Settings.PackAssets);
            }    
        }

        public void WriteAssetsToDisk(string path, bool packAssets)
        {
            if (!_isDirty.boolValue) return;

            string name;
            string dir;

            if (path != null)
            {
                dir = AssetUtils.GetDirectoryName(path);
                name = AssetUtils.GetFileName(path);
            }
            else
            {
                dir = SystemData.Settings.SavePath;
                name = _materiaSetter.gameObject.name;
                path = dir + name + ".asset";
            }

            Material material = null;
            Textures outputTextures = null;

            bool isDataAssetExisting;
            var data = AssetUtils.CreateOrReplaceScriptableObjectAsset(_materiaSetter.MateriaSetterData, path, out isDataAssetExisting);

            if (isDataAssetExisting)
            {
                if (_editMode.enumValueIndex == 0)
                {
                    outputTextures = data.Textures;
                    material = data.Material;

                    if (data.MaterialData != _materiaSetter.MaterialData)
                    {
                        var mat = Instantiate(_materiaSetter.Material);
                        mat.name = material.name;

                        if (packAssets)
                        {
                            AssetDatabase.RemoveObjectFromAsset(material);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.AddObjectToAsset(mat, data);
                            //AssetDatabase.SaveAssets();
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mat));
                            material = mat;
                        }
                        else
                        {
                            AssetUtils.CheckDirAndCreate(dir, name);
                            var folderDir = dir + "/" + name + "/";
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(material));
                            AssetDatabase.CreateAsset(mat, folderDir + name + "_Material.mat");
                            material = (Material)AssetDatabase.LoadAssetAtPath(folderDir + name + "_Material.mat", typeof(Material));
                        }
                        
                    }
                }
                else if (_editMode.enumValueIndex == 1)
                {
                    outputTextures = data.MateriaAtlas.Textures;
                    material = data.MateriaAtlas.Material;
                    name = data.MateriaAtlas.name;
                }
            }
            else
            {
                outputTextures = new Textures();
                outputTextures.CreateTextures(_materiaSetter.Textures.Size.x, _materiaSetter.Textures.Size.y);

                material = Instantiate(_materiaSetter.Material); // I'm instantiating here because Unity can't add an object to asset if it is already a part of an asset
            }

            

            if (_editMode.enumValueIndex == 0)
            {
                outputTextures.CopyPixelColors(_materiaSetter.Textures, _materiaSetter.Textures.Size.x, new Rect(0, 0, 1, 1), outputTextures.Size.x, new Rect(0, 0, 1, 1));
                outputTextures.SetNames(name);

                if (data.MateriaAtlas != null)
                {
                    data.MateriaAtlas.Textures.CopyPixelColors(_materiaSetter.Textures, _materiaSetter.Textures.Size.x, new Rect(0, 0, 1, 1), data.MateriaAtlas.Textures.Size.x, _materiaSetter.MateriaSetterData.AtlasedUVRect);
                }
            }
            else if (_editMode.enumValueIndex == 1)
            {
                outputTextures.CopyPixelColors(_materiaSetter.Textures, _materiaSetter.Textures.Size.x, _materiaSetter.UVRect, _materiaSetter.Textures.Size.x, _materiaSetter.UVRect);
                outputTextures.SetNames(name);

                data.Textures.CopyPixelColors(_materiaSetter.Textures, _materiaSetter.Textures.Size.x, _materiaSetter.UVRect, data.NativeGridSize, new Rect(0, 0, 1, 1));
            }

            

            if (packAssets)
            {
                material.name = name + "_Material";

                if (!isDataAssetExisting)
                {
                    AssetDatabase.AddObjectToAsset(material, data);
                    outputTextures.AddTexturesToAsset(data);
                }
            }
            else
            {
                AssetUtils.CheckDirAndCreate(dir, name);
                var folderDir = dir + "/" + name + "/";

                if (!isDataAssetExisting)
                {
                    AssetDatabase.CreateAsset(material, folderDir + name + "_Material.mat");
                    material = (Material)AssetDatabase.LoadAssetAtPath(folderDir + name + "_Material.mat", typeof(Material));
                    outputTextures.WriteTexturesToDisk(folderDir);
                }
                else
                {
                    //AssetDatabase.CreateAsset(material, folderDir + name + "_Material.mat");
                    //material = (Material)AssetDatabase.LoadAssetAtPath(folderDir + name + "_Material.mat", typeof(Material));
                    outputTextures.WriteTexturesToDisk(folderDir);
                }
            }

            _materiaSetter.Material = material;
            _materiaSetter.Textures = outputTextures;

            if (_editMode.enumValueIndex == 0)
            {
                data.Material = material;
                data.Textures = outputTextures;
                data.NativeMesh = _materiaSetter.Mesh;
                data.NativeGridSize = _materiaSetter.GridSize;
            }
            else if (_editMode.enumValueIndex == 1)
            {
                //data.Material = material;
                //data.Textures = outputTextures;
            }

            if (data.MateriaSlots != null)
            {
                data.MateriaSlots.Clear();
            }
            else
            {
                data.MateriaSlots = new List<MateriaSlot>();
            }
            
            data.MateriaSlots.AddRange(_materiaSetter.MateriaSlots);

            data.MaterialData = (MaterialData)_materialData.objectReferenceValue;
            data.MateriaPreset = (MateriaPreset)_materiaPreset.objectReferenceValue;
           

            _materiaSetter.SetTextures();
            _materiaSetter.MateriaSetterData = data;

            serializedObject.Update();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtils.MarkOpenPrefabSceneDirty();

            _materiaSetter.UpdateRenderer(false);

            if (_editMode.enumValueIndex == 0)
            {
                ReloadData(data);
            }
            else if (_editMode.enumValueIndex == 1)
            {
                LoadAtlas(data.MateriaAtlas);
            }
            
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
            _editMode = serializedObject.FindProperty("EditMode");
            _isDirty = serializedObject.FindProperty("IsDirty");
            _materiaAtlas = serializedObject.FindProperty("MateriaAtlas");
            _materiaPreset = serializedObject.FindProperty("MateriaPreset");
            _materiaSetterData = serializedObject.FindProperty("MateriaSetterData");
            _materialData = serializedObject.FindProperty("MaterialData");
            _material = serializedObject.FindProperty("Material");

            _materiaAtlasObjectField = root.Q<ObjectField>("MateriaAtlasObjectField");
            _materiaAtlasObjectField.objectType = typeof(MateriaAtlas);
            _materiaAtlasObjectField.SetEnabled(false);
            _materiaPresetObjectField = root.Q<ObjectField>("MateriaPresetObjectField");
            _materiaPresetObjectField.objectType = typeof(MateriaPreset);
            _materiaSetterDataObjectField = root.Q<ObjectField>("MateriaSetterDataObjectField");
            _materiaSetterDataObjectField.objectType = typeof(MateriaSetterData);
            _materialDataObjectField = root.Q<ObjectField>("MaterialDataObjectField");
            _materialDataObjectField.objectType = typeof(MaterialData);


            _reloadButton = root.Q<Button>("ReloadButton");
            _switchEditModeButton = root.Q<Button>("SwitchEditMode");
            _reloadMateriaPresetButton = root.Q<Button>("ReloadMateriaPresetButton");
            _newMateriaSetterDataButton = root.Q<Button>("NewMateriaSetterDataButton");
            _reloadMateriaSetterDataButton = root.Q<Button>("ReloadMateriaSetterDataButton");
            _overwriteMateriaSetterData = root.Q<Button>("OverwriteMateriaSetterDataButton");
            _saveAsNewMateriaSetterData = root.Q<Button>("SaveAsNewMateriaSetterDataButton");

            _atlasIndicator = root.Q<VisualElement>("AtlasIndicator");
            _presetIndicator = root.Q<VisualElement>("PresetIndicator");
            _outputIndicator = root.Q<VisualElement>("OutputIndicator");
            _dataIndicator = root.Q<VisualElement>("DataIndicator");

            _IMGUIContainer = root.Q<VisualElement>("IMGUIContainer");

            _uvIslandContainer = root.Q<VisualElement>("UVIslandContainer");

            _uvDisplayModeEnumField = root.Q<EnumField>("UVDisplayMode");
            _uvDisplayModeEnumField.Init(UVDisplayMode.BaseColor);

            _currentShaderLabel = root.Q<Label>("CurrentShaderLabel");
        }

        protected override void BindProperties()
        {
            _materiaAtlasObjectField.BindProperty(_materiaAtlas);
            _materiaPresetObjectField.BindProperty(_materiaPreset);
            _materiaSetterDataObjectField.BindProperty(_materiaSetterData);
            _materialDataObjectField.BindProperty(_materialData);

            //_uvDisplayModeEnumField.
        }

        protected override void RegisterButtons()
        {
            _reloadButton.clicked += Refresh;
            _switchEditModeButton.clicked += SwitchEditMode;
            _reloadMateriaPresetButton.clicked += ReloadPreset;
            _newMateriaSetterDataButton.clicked += NewData;
            _reloadMateriaSetterDataButton.clicked += () => { ReloadData((MateriaSetterData)_materiaSetterDataObjectField.value); };
            _overwriteMateriaSetterData.clicked += OverwriteMateriaSetterData;
            _saveAsNewMateriaSetterData.clicked += SaveAsNewMateriaSetterData;
        }
    }
}
