﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace Materiator
{
    public class MateriaAtlasEditorWindow : MateriatorEditorWindow
    {
        private static MateriaAtlasEditorWindow _window;

        private Label _selectedMateriaSettersValue;
        private ListView _materiaSetterDataListView;
        private ObjectField _atlasObjectField;

        private ToolbarSearchField _materiaSetterSearchToolbar;
        private TextField _materiaSetterSearchToolbarTextField;
        private string _searchString = "";

        private VisualElement _atlasSectionContainer;

        private Button _scanProjectButton;
        private Button _selectAtlasButton;
        private Button _loadAtlasButton;
        private Button _overwriteButton;
        private Button _saveAsNewButton;

        private List<MateriaSetter> _materiaSetters;
        private List<MateriaSetter> _compatibleMateriaSetters = new List<MateriaSetter>();
        private List<MateriaSetter> _incompatibleMateriaSetters = new List<MateriaSetter>();
        private Dictionary<MaterialData, List<MateriaSetter>> _groupsList = new Dictionary<MaterialData, List<MateriaSetter>>();
        private Dictionary<MaterialData, Material> _materialDataMaterialGroups = new Dictionary<MaterialData, Material>();

        [MenuItem("Tools/Materiator/Atlas Editor")]

        public static void OpenWindow()
        {
            _window = GetWindow<MateriaAtlasEditorWindow>("Atlas Editor");
            _window.minSize = new Vector2(400f, 600f);
        }

        private void OnEnable()
        {
            InitializeEditorWindow<MateriaAtlasEditorWindow>();
            serializedObject = new SerializedObject(this);

            _atlasSectionContainer.visible = false;

            RegisterCallbacks();
        }

        private void GenerateListView(List<MateriaSetter> materiaSettersList)
        {
            _materiaSetterDataListView.Clear();
            // The "makeItem" function will be called as needed
            // when the ListView needs more items to render
            Func<VisualElement> makeItem = () => new VisualElement();

            // As the user scrolls through the list, the ListView object
            // will recycle elements created by the "makeItem"
            // and invoke the "bindItem" callback to associate
            // the element with the matching data item (specified as an index in the list)
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                var ms = _materiaSetterDataListView.itemsSource.Cast<MateriaSetter>().ToList();
                var go = ms[i].transform.root.gameObject;

                var prefabGUIContent = EditorGUIUtility.ObjectContent(go, go.GetType());
                // BUG?: If I cache materiaSetter GUIContent here and use it in the same manner as prefabGUIContent, materiaSetter's GUICOntent replaces prefabGUIContent icon and text in list view
                //----------------------------------------------------------------------------------------

                var item = new AtlasEditorListEntry((i + 1).ToString(), prefabGUIContent.image as Texture2D, prefabGUIContent.text,
                    EditorGUIUtility.ObjectContent(ms[i], ms[i].GetType()).image as Texture2D, ms[i].name,
                    EditorGUIUtility.ObjectContent(ms[i].MaterialData, ms[i].MaterialData.GetType()).image as Texture2D, ms[i].MaterialData.name);
                item.RemoveListEntryButton.clicked += () => RemoveListEntry(ms[i]);

                e.Add(item);
            }; 

            _materiaSetterDataListView.makeItem = makeItem;
            _materiaSetterDataListView.bindItem = bindItem;
            _materiaSetterDataListView.itemsSource = materiaSettersList;

            // Callback invoked when the user double clicks an item
            _materiaSetterDataListView.onItemsChosen += (items) => FocusListEntry((MateriaSetter)items.FirstOrDefault());
        }

        private void FilterMateriaSetterListView(ChangeEvent<string> e)
        {
            _searchString = e.newValue.ToLower();

            var filteredList = new List<MateriaSetter>();

            foreach (var item in _materiaSetters)
            {
                if (MatchesSearchInput(item, _searchString))
                {
                    filteredList.Add(item);
                }
            }

            _materiaSetterDataListView.itemsSource = filteredList;
            _materiaSetterDataListView.Refresh();
        }

        private bool MatchesSearchInput(MateriaSetter ms, string searchInput)
        {
            if (ms.name.ToLower().Contains(searchInput))
            {
                return true;
            }
            else if (ms.transform.root.gameObject.name.ToLower().Contains(searchInput))
            {
                return true;
            }
            else if (ms.MateriaSetterData.MaterialData.name.ToLower().Contains(searchInput))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void FocusListEntry(MateriaSetter ms)
        {
            var go = ms.gameObject;

            Selection.activeObject = go;
            EditorGUIUtility.PingObject(go);
        }

        private void RemoveListEntry(MateriaSetter item)
        {
            _materiaSetters.Remove(item);

            _selectedMateriaSettersValue.text = _materiaSetters.Count.ToString();
            _materiaSetterDataListView.Refresh();
        }

        private void Scan()
        {
            LoadPrefabs((MateriaAtlas)_atlasObjectField.value);

            GenerateListView(_materiaSetters);

            _atlasSectionContainer.visible = false;
        }

        private void SelectAtlas()
        {
            _atlasSectionContainer.visible = true;
        }

        private void LoadAtlas()
        {
            LoadPrefabs((MateriaAtlas)_atlasObjectField.value);

            GenerateListView(_materiaSetters);
        }

        private void LoadPrefabs(MateriaAtlas atlas = null)
        {
            if (atlas == null)
            {
                //ResetAtlasGenerator(true);
                _materiaSetters = AssetUtils.FindAllComponentsInPrefabs<MateriaSetter>();
                CheckMateriaSettersCompatibility(_materiaSetters);
            }
            else
            {


                //_materiaSetters = new List<MateriaSetter>();
                _materiaSetters = atlas.AtlasEntries.Keys.ToList();
                //foreach (var msd in atlas.AtlasEntry.Keys)
                //{
                //   _materiaSetters.Add(msd);
                //}
                foreach (var item in _materiaSetters)
                {
                    Debug.Log(item.MateriaSetterData.name);
                }

                //_materiaSetters = AssetUtils.FindAllComponentsInPrefabs<MateriaSetter>().Where(d => d.MateriaSetterData == msd);
            }

            _selectedMateriaSettersValue.text = _materiaSetters.Count.ToString();
        }

        private void CheckMateriaSettersCompatibility(ICollection<MateriaSetter> materiaSetters)
        {
            foreach (var ms in materiaSetters)
            {
                var groupMembers = new List<MateriaSetter>();
                //-----TODO: Have an option to create atlas without having a preset set?
                if (AtlasFactory.CheckMateriaSetterCompatibility(ms))
                {
                    if (PrefabUtility.IsPartOfPrefabAsset(ms)
                    && !PrefabUtility.IsPartOfPrefabInstance(ms) || PrefabUtility.IsPartOfVariantPrefab(ms))
                    {
                        _compatibleMateriaSetters.Add(ms);

                        if (!_groupsList.ContainsKey(ms.MateriaSetterData.MaterialData))
                        {
                            _groupsList.Add(ms.MateriaSetterData.MaterialData, groupMembers);
                        }
                        _groupsList[ms.MateriaSetterData.MaterialData].Add(ms);

                        if (!_materialDataMaterialGroups.ContainsKey(ms.MateriaSetterData.MaterialData))
                            _materialDataMaterialGroups.Add(ms.MateriaSetterData.MaterialData, ms.MateriaSetterData.Material);
                    }
                }
                else
                    _incompatibleMateriaSetters.Add(ms);
            }
        }

        private void Overwrite()
        {

        }

        private void SaveAsNew()
        {
            var path = EditorUtility.SaveFilePanelInProject("Choose where to save", "", "asset", "asset");
            if (path.Length != 0)
            {
                var i = 0;
                foreach (var kvp in _groupsList)
                {
                    AtlasFactory.CreateAtlas(kvp, _materialDataMaterialGroups[kvp.Key], path, false, "_newPrefabSuffix");
                    i++;
                }
            }
        }

        private void RegisterCallbacks()
        {
            _materiaSetterSearchToolbarTextField.RegisterCallback<ChangeEvent<string>>(e =>
            {
                FilterMateriaSetterListView(e);
            });
        }

        protected override void GetProperties()
        {
            _materiaSetterDataListView = root.Q<ListView>("MateriaSetterDataListView");

            _materiaSetterSearchToolbar = root.Q<ToolbarSearchField>("MateriaSetterSearchToolbar");
            _materiaSetterSearchToolbarTextField = _materiaSetterSearchToolbar.Q<TextField>();

            _selectedMateriaSettersValue = root.Q<Label>("SelectedMateriaSettersValue");

            _atlasSectionContainer = root.Q<VisualElement>("AtlasSectionContainer");

            _atlasObjectField = root.Q<ObjectField>("AtlasObjectField");
            _atlasObjectField.objectType = typeof(MateriaAtlas);

            _scanProjectButton = root.Q<Button>("ScanProjectButton");
            _selectAtlasButton = root.Q<Button>("SelectAtlasButton");
            _loadAtlasButton = root.Q<Button>("LoadAtlasButton");
            _overwriteButton = root.Q<Button>("OverwriteButton");
            _saveAsNewButton = root.Q<Button>("SaveAsNewButton");
        }

        protected override void BindProperties()
        {
            _materiaSetterDataListView.Bind(serializedObject);
        }

        protected override void RegisterButtons()
        {
            _scanProjectButton.clicked += Scan;
            _selectAtlasButton.clicked += SelectAtlas;
            _loadAtlasButton.clicked += LoadAtlas;
            _overwriteButton.clicked += Overwrite;
            _saveAsNewButton.clicked += SaveAsNew;
        }
    }
}