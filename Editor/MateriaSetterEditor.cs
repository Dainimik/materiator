using System;
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
        public MateriaSetter MateriaSetter;

        private ReorderableList _materiaReorderableList;

        public Action<bool> OnDirtyChanged;

        #region Serialized Properties

        public SerializedProperty EditMode;
        public SerializedProperty IsDirty;
        private SerializedProperty _materiaAtlas;
        public SerializedProperty MateriaPreset;
        
        public SerializedProperty Material;

        #endregion

        private ObjectField _materiaAtlasObjectField;
        private ObjectField _materiaPresetObjectField;

        private Button _reloadButton;
        private Button _switchEditModeButton;
        private Button _reloadMateriaPresetButton;
        
        private Button _overwriteMateriaSetterData;
        private Button _saveAsNewMateriaSetterData;

        private VisualElement _atlasIndicator;
        private VisualElement _presetIndicator;
        private VisualElement _outputIndicator;
        

        private VisualElement _IMGUIContainer;

        public VisualElement Root;

        private UVInspector _uvInspector;

        public DataSection DataSection;

        private void OnEnable()
        {
            MateriaSetter = (MateriaSetter)target;

            InitializeEditor<MateriaSetter>();

            Root = new VisualElement();

            if (Initialize())
            {
                _uvInspector = new UVInspector(MateriaSetter, Root);

                DrawAtlasSection();
                DrawPresetSection();
                DataSection = new DataSection(this);
                DrawOutputSection();
                DrawIMGUI();
            }
        }

        private void OnDisable()
        {
            MateriaSetter = (MateriaSetter)target;
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

                Root = root;

                return true;
            }
            else
            {
                return false;
            }
        }

        private void Refresh()
        {
            MateriaSetter.Refresh();
            _uvInspector.DrawUVInspector(true);
        }

        public void ResetMateriaSetter()
        {
            MateriaSetter.ResetMateriaSetter();

            SetMateriaSetterDirty(true);
        }

        public void SetMateriaSetterDirty(bool value)
        {
            serializedObject.Update();
            if (value)
            {
                if (!IsDirty.boolValue)
                {
                    IsDirty.boolValue = true;

                    CreateEditModeData(EditMode.enumValueIndex);
                }
            }
            else
            {
                if (IsDirty.boolValue)
                {
                    IsDirty.boolValue = false;
                }  
            }

            _uvInspector.DrawUVInspector(true);

            serializedObject.ApplyModifiedProperties();

            OnDirtyChanged?.Invoke(value);
        }

        private void SwitchEditMode()
        {
            if (MateriaSetter.EditMode == Materiator.EditMode.Native)
            {
                LoadAtlas(MateriaSetter.MateriaSetterData.MateriaAtlas);
            }
            else if (MateriaSetter.EditMode == Materiator.EditMode.Atlas)
            {
                UnloadAtlas();
            }
        }

        private void DrawIMGUI()
        {
            IMGUIContainer defaultInspector = new IMGUIContainer(() => IMGUI());
            root.Add(defaultInspector);

            _materiaReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("MateriaSlots"), false, true, false, false);
            DrawMateriaReorderableList();
            IMGUIContainer materiaReorderableList = new IMGUIContainer(() => MateriaReorderableList());
            _IMGUIContainer.Add(materiaReorderableList);
        }

        private void IMGUI()
        {
            if (IsDirty.boolValue == true)
            {
                _outputIndicator.style.backgroundColor = SystemData.Settings.GUIRed;
            }
            else
            {
                _outputIndicator.style.backgroundColor = SystemData.Settings.GUIGreen;
            }

            DrawDefaultInspector();
        }

        private void DrawAtlasSection()
        {
            OnMateriaAtlasChanged();

            _materiaAtlasObjectField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                OnMateriaAtlasChanged();
            });
        }

        private void DrawPresetSection()
        {
            OnMateriaPresetChanged();

            _materiaPresetObjectField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                OnMateriaPresetChanged();
            });
        }

        private void DrawOutputSection()
        {
            if (DataSection.MateriaSetterData.objectReferenceValue == null)
            {
                _overwriteMateriaSetterData.SetEnabled(false);
            }
            else
            {
                _overwriteMateriaSetterData.SetEnabled(true);
            }
        }

        public void LoadAtlas(MateriaAtlas atlas)
        {
            if (atlas != null)
            {
                MateriaSetter.MateriaSlots = MateriaSetter.MateriaSetterData.MateriaSlots;

                MateriaSetter.LoadAtlas(atlas);
            }

            SetMateriaSetterDirty(false);
        }

        public void UnloadAtlas()
        {
            MateriaSetter.UnloadAtlas();
        }

        private void LoadPreset(MateriaPreset preset)
        {
            List<MateriaSlot> materiaSlots;
            if (MateriaSetter.MateriaSetterData != null)
            {
                materiaSlots = MateriaSetter.MateriaSetterData.MateriaSlots;
            }
            else
            {
                materiaSlots = MateriaSetter.MateriaSlots;
            }
            var same = AreMateriasSameAsPreset(preset, MateriaSetter.MateriaSlots);

            if (!same)
            {
                SetMateriaSetterDirty(true);

                if (preset != null)
                {
                    _reloadMateriaPresetButton.visible = true;
                    MateriaSetter.LoadPreset(preset);
                }
                else
                {
                    _reloadMateriaPresetButton.visible = false;
                    MateriaSetter.LoadPreset(null);
                }
                serializedObject.Update();

                MateriaSetter.UpdateColorsOfAllTextures();
            }

            _uvInspector.DrawUVInspector(true);
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
                EditorGUI.LabelField(new Rect(rect.x + 170f, rect.y, 100f, 20f), new GUIContent("Materia " + "(" + MateriaSetter.MateriaSlots.Count + ")"), EditorStyles.boldLabel);
            };

            _materiaReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                serializedObject.Update();
                var element = _materiaReorderableList.serializedProperty.GetArrayElementAtIndex(index);
                var elementID = element.FindPropertyRelative("ID");
                var elementMateria = element.FindPropertyRelative("Materia").objectReferenceValue as Materia;
                var materiaTag = MateriaSetter.MateriaSlots[index].Tag;

                Rect r = new Rect(rect.x, rect.y, 22f, 22f);

                var color = elementMateria.BaseColor;
                var emission = elementMateria.EmissionColor;

                color.a = 255;
                emission.a = 255;

                if (elementMateria.IsEmissive)
                {
                    var texE = new Texture2D(4, 4);
                    Rect texRE = new Rect(rect.x + 145f, rect.y, 20f, 20f);
                    GUI.DrawTexture(texRE, texE, ScaleMode.StretchToFill, false, 0, emission, 0, 0);
                }

                var tex = new Texture2D(4, 4);

                EditorGUI.LabelField(r, new GUIContent((elementID.intValue + 1).ToString()));
                EditorGUI.LabelField(new Rect(rect.x + 120f, rect.y, rect.width, rect.height), new GUIContent(MateriaSetter.MateriaSlots[index].Materia.PreviewIcon));

                serializedObject.Update();

                int _materiaTagIndex = 0;
                EditorGUI.BeginChangeCheck();
                _materiaTagIndex = EditorGUI.Popup(new Rect(rect.x + 25f, rect.y, 95f, rect.height), SystemData.Settings.MateriaTags.MateriaTagsList.IndexOf(SystemData.Settings.MateriaTags.MateriaTagsList.Where(t => t.Name == materiaTag.Name).FirstOrDefault()), SystemData.Settings.MateriaTags.MateriaTagNamesArray, EditorStyles.popup);
                if (EditorGUI.EndChangeCheck())
                {
                    SetMateriaSetterDirty(true);
                    var newTag = SystemData.Settings.MateriaTags.MateriaTagsList[_materiaTagIndex];
                    Undo.RegisterCompleteObjectUndo(MateriaSetter, "Change Materia Tag");
                    MateriaSetter.MateriaSlots[index].Tag = newTag;
                    serializedObject.Update();
                }

                EditorGUI.BeginChangeCheck();
                elementMateria = (Materia)EditorGUI.ObjectField(new Rect(rect.x + 170f, rect.y, rect.width - 195f, rect.height), elementMateria, typeof(Materia), false);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(MateriaSetter, "Change Materia");
                    SetMateriaSetterDirty(true);

                    if (elementMateria == null)
                        elementMateria = SystemData.Settings.DefaultMateria;
                    else
                        MateriaSetter.MateriaSlots[index].Materia = elementMateria;

                    serializedObject.Update();
                    _uvInspector.DrawUVInspector(true);

                    MateriaSetter.UpdateColorsOfAllTextures();

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

        Texture2D _highlightedTexture;
        private void HandleMateriaSlotSelection(int index, bool selected)
        {
            var originalTexture = MateriaSetter.Textures.Color;

            if (_highlightedTexture == null)
            {
                _highlightedTexture = new Texture2D(MateriaSetter.Textures.Color.width, MateriaSetter.Textures.Color.height, TextureFormat.RGBA32, false);
                EditorUtility.CopySerialized(originalTexture, _highlightedTexture);
                _highlightedTexture.filterMode = SystemData.Settings.FilterMode;
            }

            var colors = originalTexture.GetPixels32();
            var rectInt = Utils.GetRectIntFromRect(MateriaSetter.GridSize, MateriaSetter.FilteredRects[index]);

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = SystemData.Settings.HighlightColor;
            }

            if (selected)
            {
                _highlightedTexture.SetPixels32(rectInt.x, rectInt.y, rectInt.width, rectInt.height, colors);
                _highlightedTexture.Apply();

                MateriaSetter.Renderer.sharedMaterial.SetTexture(MateriaSetter.MaterialData.ShaderData.MainTexturePropertyName, _highlightedTexture);
            }
            else
            {
                _highlightedTexture.SetPixels32(originalTexture.GetPixels32());
                _highlightedTexture.Apply();

                MateriaSetter.Renderer.sharedMaterial.SetTexture(MateriaSetter.MaterialData.ShaderData.MainTexturePropertyName, originalTexture);
            }
        }

        private void CreateEditModeData(int editMode)
        {
            if (DataSection.MateriaSetterData.objectReferenceValue != null)
            {
                var newTextures = new Textures();

                Textures textures = null;

                if (editMode == 0)
                {
                    textures = MateriaSetter.MateriaSetterData.Textures;
                }
                else if (editMode == 1)
                {
                    textures = MateriaSetter.MateriaSetterData.MateriaAtlas.Textures;
                }

                newTextures.CreateTextures(textures.Size.x, textures.Size.y);
                var mat = Instantiate(Material.objectReferenceValue);
                newTextures.CopyPixelColors(textures, textures.Size, SystemData.Settings.UVRect, newTextures.Size, SystemData.Settings.UVRect);

                if (name != null)
                    mat.name = name;

                Material.objectReferenceValue = mat;

                serializedObject.ApplyModifiedProperties();

                MateriaSetter.Textures = newTextures;
                MateriaSetter.SetTextures();

                var newMateriaSlots = new List<MateriaSlot>();

                foreach (var item in MateriaSetter.MateriaSetterData.MateriaSlots)
                {
                    newMateriaSlots.Add(new MateriaSlot(item.ID, item.Materia, item.Tag));
                }

                MateriaSetter.MateriaSlots = newMateriaSlots;
                serializedObject.Update();
                MateriaSetter.UpdateRenderer();
                serializedObject.Update();
            }
        }

        private void ReloadPreset()
        {
            LoadPreset((MateriaPreset)_materiaPresetObjectField.value);

            OnMateriaPresetChanged();
        }

        private void OnMateriaAtlasChanged()
        {
            if (MateriaSetter.MateriaSetterData != null)
            {
                if (MateriaSetter.MateriaAtlas != null)
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

            if (MateriaSetter.MateriaAtlas == null)
            {
                _switchEditModeButton.SetEnabled(false);
            }
            else
            {
                _switchEditModeButton.SetEnabled(true);
            }

            _switchEditModeButton.text = EditMode.enumNames[EditMode.enumValueIndex] + " Mode";
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
                
                if (AreMateriasSameAsPreset((MateriaPreset)_materiaPresetObjectField.value, MateriaSetter.MateriaSlots))
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
                        if (materiaSlots[i].Tag.Name == preset.MateriaPresetItemList[j].Tag.Name)
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

        private void OverwriteMateriaSetterData()
        {
            if (EditorUtility.DisplayDialog("Overwrite current data?", "Are you sure you want to overwrite " +  DataSection.MateriaSetterData.objectReferenceValue.name + " with current settings?", "Yes", "No"))
            {
                MateriaDataFactory.WriteAssetsToDisk(this, AssetDatabase.GetAssetPath(MateriaSetter.MateriaSetterData), SystemData.Settings.PackAssets);
                
            }
        }
        private void SaveAsNewMateriaSetterData()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save data", MateriaSetter.gameObject.name, "asset", "asset");
            if (path.Length != 0)
            {
                MateriaDataFactory.WriteAssetsToDisk(this, path, SystemData.Settings.PackAssets);
            }    
        }

        protected override void GetProperties()
        {
            EditMode = serializedObject.FindProperty("EditMode");
            IsDirty = serializedObject.FindProperty("IsDirty");
            _materiaAtlas = serializedObject.FindProperty("MateriaAtlas");
            MateriaPreset = serializedObject.FindProperty("MateriaPreset");
            Material = serializedObject.FindProperty("Material");

            _materiaAtlasObjectField = root.Q<ObjectField>("MateriaAtlasObjectField");
            _materiaAtlasObjectField.objectType = typeof(MateriaAtlas);
            _materiaAtlasObjectField.SetEnabled(false);
            _materiaPresetObjectField = root.Q<ObjectField>("MateriaPresetObjectField");
            _materiaPresetObjectField.objectType = typeof(MateriaPreset);

            _reloadButton = root.Q<Button>("ReloadButton");
            _switchEditModeButton = root.Q<Button>("SwitchEditMode");
            _reloadMateriaPresetButton = root.Q<Button>("ReloadMateriaPresetButton");
            _overwriteMateriaSetterData = root.Q<Button>("OverwriteMateriaSetterDataButton");
            _saveAsNewMateriaSetterData = root.Q<Button>("SaveAsNewMateriaSetterDataButton");

            _atlasIndicator = root.Q<VisualElement>("AtlasIndicator");
            _presetIndicator = root.Q<VisualElement>("PresetIndicator");
            _outputIndicator = root.Q<VisualElement>("OutputIndicator");

            _IMGUIContainer = root.Q<VisualElement>("IMGUIContainer");
        }

        protected override void BindProperties()
        {
            _materiaAtlasObjectField.BindProperty(_materiaAtlas);
            _materiaPresetObjectField.BindProperty(MateriaPreset);
        }

        protected override void RegisterButtons()
        {
            _reloadButton.clicked += Refresh;
            _switchEditModeButton.clicked += SwitchEditMode;
            _reloadMateriaPresetButton.clicked += ReloadPreset;
            _overwriteMateriaSetterData.clicked += OverwriteMateriaSetterData;
            _saveAsNewMateriaSetterData.clicked += SaveAsNewMateriaSetterData;
        }
    }
}
