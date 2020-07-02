﻿using System.Collections.Generic;
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

        private SerializedProperty _editMode;
        private SerializedProperty _isDirty;
        private SerializedProperty _materiaAtlas;
        private SerializedProperty _materiaPreset;
        private SerializedProperty _materiaSetterData;
        private SerializedProperty _shaderData;
        private SerializedProperty _material;

        #endregion

        private ObjectField _materiaAtlasObjectField;
        private ObjectField _materiaPresetObjectField;
        private ObjectField _materiaSetterDataObjectField;
        private ObjectField _shaderDataObjectField;

        private Button _reloadButton;
        private Button _switchEditModeButton;
        private Button _reloadMateriaAtlasButton;
        private Button _reloadMateriaPresetButton;
        private Button _newMateriaSetterDataButton;
        private Button _reloadMateriaSetterDataButton;
        private Button _overwriteMateriaSetterData;
        private Button _saveAsNewMateriaSetterData;

        private VisualElement _presetIndicator;
        private VisualElement _outputIndicator;
        private VisualElement _dataIndicator;

        private VisualElement _IMGUIContainer;

        private VisualElement _uvIslandContainer;
        private VisualElement _uvGridItem;
        private EnumField _uvDisplayModeEnumField;

        private Label _currentShaderLabel;

        private void OnEnable()
        {
            _materiaSetter = (MateriaSetter)target;

            Initialize();
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
        }

        private void SetMateriaSetterDirty(bool value)
        {
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
                _outputIndicator.style.backgroundColor = new Color(0.75f, 0f, 0f, 1f);
                _dataIndicator.style.backgroundColor = new Color(0.75f, 0f, 0f, 1f);
            }
            else
            {
                _outputIndicator.style.backgroundColor = new Color(0f, 0.75f, 0f, 1f);
                _dataIndicator.style.backgroundColor = new Color(0f, 0.75f, 0f, 1f);
            }

            DrawDefaultInspector();
        }

        private void ResetMateriaSetter()
        {
            _materiaSetter.ResetMateriaSetter();
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

            _currentShaderLabel.text = _materiaSetter.ShaderData.Shader.name;

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
                OnMateriaSetterDataChanged();
            });

            _uvDisplayModeEnumField.RegisterCallback<ChangeEvent<System.Enum>>(e =>
            {
                _uvDisplayModeEnumField.value = e.newValue;
                DrawUVInspector(true);
            });

            _shaderDataObjectField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                OnShaderDataChanged();
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
                        Color color = Utils.Settings.DefaultMateria.BaseColor;
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
                // this is necessary because atlas generation
                //OnMateriaSetterDataChanged();

                _reloadMateriaAtlasButton.SetEnabled(true);
                _materiaSetter.LoadAtlas(atlas);
            }
            else
            {
                _reloadMateriaAtlasButton.SetEnabled(false);
            } 
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
                _material.objectReferenceValue = Instantiate(_material.objectReferenceValue);
                newTextures.CopyPixelColors(textures, textures.Size.x, new Rect(0, 0, 1, 1), newTextures.Size.x, new Rect(0, 0, 1, 1));

                if (name != null)
                    _material.objectReferenceValue.name = name;

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
                _materiaTagIndex = EditorGUI.Popup(new Rect(rect.x + 25f, rect.y, 95f, rect.height), Utils.MateriaTags.MateriaTagsList.IndexOf(materiaTag.stringValue), Utils.MateriaTags.MateriaTagsArray, EditorStyles.popup);
                if (EditorGUI.EndChangeCheck())
                {
                    SetMateriaSetterDirty(true);
                    var newTag = Utils.MateriaTags.MateriaTagsList[_materiaTagIndex];
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
                        elementMateria = Utils.Settings.DefaultMateria;
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
                _highlightedTexture.filterMode = Utils.Settings.FilterMode;
            }

            var colors = originalTexture.GetPixels32();
            var rectInt = Utils.GetRectIntFromRect(_materiaSetter.GridSize, _materiaSetter.FilteredRects[index]);

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
            DrawUVInspector(true);
        }

        private void SwitchEditMode()
        {
            if (_materiaSetter.EditMode == EditMode.Native)
            {
                _materiaSetter.LoadAtlas(_materiaSetter.MateriaSetterData.MateriaAtlas);
            }
            else if (_materiaSetter.EditMode == EditMode.Atlas)
            {
                _materiaSetter.UnloadAtlas();
            }

            //SetMateriaSetterDirty(true);
            //_materiaSetter.AnalyzeMesh();
            //_materiaSetter.UpdateColorsOfAllTextures();

            //_materiaSetter.MateriaSlots = _materiaSetter.MateriaSetterData.MateriaSlots;
        }

        private void ReloadAtlas()
        {
            LoadAtlas((MateriaAtlas)_materiaAtlasObjectField.value);
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

        private void ReloadData()
        {
            OnMateriaSetterDataChanged();
        }

        private void OnMateriaAtlasChanged()
        {
            if (_materiaAtlasObjectField.value == null)
            {
                _reloadMateriaAtlasButton.SetEnabled(false);
            }
            else
            {
                _reloadMateriaAtlasButton.SetEnabled(true);
            }

            if (_materiaSetter.MateriaAtlas  == null)
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
                _presetIndicator.style.backgroundColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            }
            else
            {
                _reloadMateriaPresetButton.SetEnabled(true);
                
                if (AreMateriasSameAsPreset((MateriaPreset)_materiaPresetObjectField.value, _materiaSetter.MateriaSlots))
                {
                    _presetIndicator.style.backgroundColor = new Color(0f, 0.75f, 0f, 1f);
                }
                else
                {
                    _presetIndicator.style.backgroundColor = new Color(0.75f, 0f, 0f, 1f);
                }
            }
        }

        public bool AreMateriasSameAsPreset(MateriaPreset preset, List<MateriaSlot> materiaSlots)
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

        private void OnMateriaSetterDataChanged()
        {
            if (_materiaSetterDataObjectField.value != null)
            {
                _editMode.enumValueIndex = 0;

                _materiaSetterData.objectReferenceValue = _materiaSetterDataObjectField.value;
                var data = (MateriaSetterData)_materiaSetterDataObjectField.value;

                serializedObject.ApplyModifiedProperties();

                Utils.ShallowCopyFields(data, _materiaSetter);
                _materiaSetter.Mesh = data.NativeMesh;
                //_materiaSetter.GridSize = data.NativeGridSize;

                
                serializedObject.Update();

                _materiaSetter.UpdateRenderer();
                _materiaSetter.GenerateMateriaSlots();

                //_materiaSetter.UpdateColorsOfAllTextures();
            }
            else
            {
                _reloadMateriaSetterDataButton.SetEnabled(false);

                _materiaSetterDataObjectField.value = _materiaSetterData.objectReferenceValue;
            }

            SetMateriaSetterDirty(false);
        }

        private void OnShaderDataChanged()
        {
            SetMateriaSetterDirty(true);

            var shader = ((ShaderData)_shaderDataObjectField.value).Shader;

            _materiaSetter.Renderer.sharedMaterial.shader = shader;
            _currentShaderLabel.text = shader.name;

            serializedObject.ApplyModifiedProperties();
        }

        private void OverwriteMateriaSetterData()
        {
            if (EditorUtility.DisplayDialog("Overwrite current data?", "Are you sure you want to overwrite " + _materiaSetterData.objectReferenceValue.name + " with current settings?", "Yes", "No"))
            {
                WriteAssetsToDisk(AssetDatabase.GetAssetPath(_materiaSetter.MateriaSetterData), Utils.Settings.PackAssets);
                
            }
        }
        private void SaveAsNewMateriaSetterData()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save data", _materiaSetter.gameObject.name, "asset", "asset");
            if (path.Length != 0)
            {
                WriteAssetsToDisk(path, Utils.Settings.PackAssets);
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

            //texs = _materiaSetter.Textures.CloneTextures(Utils.Settings.FilterMode);
            

            if (_editMode.enumValueIndex == 0)
            {
                outputTextures.CopyPixelColors(_materiaSetter.Textures, _materiaSetter.Textures.Size.x, new Rect(0, 0, 1, 1), outputTextures.Size.x, new Rect(0, 0, 1, 1));
                outputTextures.SetNames(name);

                if (data.MateriaAtlas != null)
                {
                    data.MateriaAtlas.Textures.CopyPixelColors(_materiaSetter.Textures, _materiaSetter.Textures.Size.x, new Rect(0, 0, 1, 1), data.MateriaAtlas.Textures.Size.x, _materiaSetter.AtlasedUVRect);
                }
            }
            else if (_editMode.enumValueIndex == 1)
            {
                outputTextures.CopyPixelColors(_materiaSetter.Textures, _materiaSetter.Textures.Size.x, _materiaSetter.AtlasedUVRect, _materiaSetter.Textures.Size.x, _materiaSetter.AtlasedUVRect);
                outputTextures.SetNames(name);

                data.Textures.CopyPixelColors(_materiaSetter.Textures, _materiaSetter.Textures.Size.x, _materiaSetter.AtlasedUVRect, data.NativeGridSize, new Rect(0, 0, 1, 1));
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
                data.NativeGridSize = _materiaSetter.NativeGridSize;
            }
            else if (_editMode.enumValueIndex == 1)
            {
                //data.Material = material;
                //data.Textures = outputTextures;
            }

            data.MateriaSlots = _materiaSetter.MateriaSlots;
            data.ShaderData = (ShaderData)_shaderData.objectReferenceValue;
            data.MateriaPreset = (MateriaPreset)_materiaPreset.objectReferenceValue;
           



            _materiaSetter.SetTextures();
            _materiaSetter.MateriaSetterData = data;

            serializedObject.Update();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtils.MarkOpenPrefabSceneDirty();

            _materiaSetter.UpdateRenderer(false);
            SetMateriaSetterDirty(false);
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
            _shaderData = serializedObject.FindProperty("ShaderData");
            _material = serializedObject.FindProperty("Material");

            _materiaAtlasObjectField = root.Q<ObjectField>("MateriaAtlasObjectField");
            _materiaAtlasObjectField.objectType = typeof(MateriaAtlas);
            _materiaPresetObjectField = root.Q<ObjectField>("MateriaPresetObjectField");
            _materiaPresetObjectField.objectType = typeof(MateriaPreset);
            _materiaSetterDataObjectField = root.Q<ObjectField>("MateriaSetterDataObjectField");
            _materiaSetterDataObjectField.objectType = typeof(MateriaSetterData);
            _shaderDataObjectField = root.Q<ObjectField>("ShaderDataObjectField");
            _shaderDataObjectField.objectType = typeof(ShaderData);


            _reloadButton = root.Q<Button>("ReloadButton");
            _switchEditModeButton = root.Q<Button>("SwitchEditMode");
            _reloadMateriaAtlasButton = root.Q<Button>("ReloadMateriaAtlasButton");
            _reloadMateriaPresetButton = root.Q<Button>("ReloadMateriaPresetButton");
            _newMateriaSetterDataButton = root.Q<Button>("NewMateriaSetterDataButton");
            _reloadMateriaSetterDataButton = root.Q<Button>("ReloadMateriaSetterDataButton");
            _overwriteMateriaSetterData = root.Q<Button>("OverwriteMateriaSetterDataButton");
            _saveAsNewMateriaSetterData = root.Q<Button>("SaveAsNewMateriaSetterDataButton");

            _presetIndicator = root.Q<VisualElement>("PresetIndicator");
            _outputIndicator = root.Q<VisualElement>("OutputIndicator");
            _dataIndicator = root.Q<VisualElement>("DataIndicator");

            _IMGUIContainer = root.Q<VisualElement>("IMGUIContainer");

            _uvIslandContainer = root.Q<VisualElement>("UVIslandContainer");
            _uvGridItem = root.Q<VisualElement>("UVGridItem");

            _uvDisplayModeEnumField = root.Q<EnumField>("UVDisplayMode");
            _uvDisplayModeEnumField.Init(UVDisplayMode.BaseColor);

            _currentShaderLabel = root.Q<Label>("CurrentShaderLabel");
        }

        protected override void BindProperties()
        {
            _materiaAtlasObjectField.BindProperty(_materiaAtlas);
            _materiaPresetObjectField.BindProperty(_materiaPreset);
            _materiaSetterDataObjectField.BindProperty(_materiaSetterData);
            _shaderDataObjectField.BindProperty(_shaderData);

            //_uvDisplayModeEnumField.
        }

        protected override void RegisterButtons()
        {
            _reloadButton.clicked += Refresh;
            _switchEditModeButton.clicked += SwitchEditMode;
            _reloadMateriaAtlasButton.clicked += ReloadAtlas;
            _reloadMateriaPresetButton.clicked += ReloadPreset;
            _newMateriaSetterDataButton.clicked += NewData;
            _reloadMateriaSetterDataButton.clicked += ReloadData;
            _overwriteMateriaSetterData.clicked += OverwriteMateriaSetterData;
            _saveAsNewMateriaSetterData.clicked += SaveAsNewMateriaSetterData;
        }
    }
}
