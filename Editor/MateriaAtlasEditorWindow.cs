using UnityEngine;
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

        private EditMode _editMode;

        private VisualElement _materiaSetterListViewDropArea;
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
        private Button _removeAtlasData;
        private Button _overwriteButton;
        private Button _saveAsNewButton;

        private MateriaAtlas _atlas;

        private List<MateriaSetter> _materiaSetters = new List<MateriaSetter>();
        private List<MateriaSetter> _compatibleMateriaSetters = new List<MateriaSetter>();
        private List<MateriaSetter> _incompatibleMateriaSetters = new List<MateriaSetter>();
        private Dictionary<MaterialData, List<MateriaSetter>> _groupsList = new Dictionary<MaterialData, List<MateriaSetter>>();
        private Dictionary<MaterialData, Material> _materialDataMaterialGroups = new Dictionary<MaterialData, Material>();

        private List<MateriaSetter> _selectedListViewMateriaSetters = new List<MateriaSetter>();

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

            SwitchEditMode(EditMode.Native);
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
                item.RemoveListEntryButton.clicked += () => RemoveListEntry(ms[i], materiaSettersList);

                e.Add(item);
            }; 

            _materiaSetterDataListView.makeItem = makeItem;
            _materiaSetterDataListView.bindItem = bindItem;
            _materiaSetterDataListView.itemsSource = materiaSettersList;

            // Callback invoked when the user double clicks an item
            _materiaSetterDataListView.onItemsChosen += (items) =>
            {
                FocusListEntry((MateriaSetter)items.FirstOrDefault()); //This line doesn't work on Unity 2019.4
            };

            _materiaSetterDataListView.onSelectionChange += (items) =>
            {
                _selectedListViewMateriaSetters.Clear();
                _selectedListViewMateriaSetters = items.OfType<MateriaSetter>().ToList();
            };
        }

        private void FilterMateriaSetterListView(string e)
        {
            _searchString = e.ToLower();

            var filteredList = new List<MateriaSetter>();

            foreach (var item in _compatibleMateriaSetters)
            {
                if (MatchesSearchInput(item, _searchString))
                {
                    filteredList.Add(item);
                }
            }

            if (filteredList.Count != _compatibleMateriaSetters.Count)
            {
                _materiaSetterDataListView.itemsSource = filteredList;
            }
            else
            {
                _materiaSetterDataListView.itemsSource = _compatibleMateriaSetters;
            }
            
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

        private void RemoveListEntry(MateriaSetter item, List<MateriaSetter> list)
        {
            list.Remove(item);
            _materiaSetters.Remove(item);

            _selectedMateriaSettersValue.text = _materiaSetters.Count.ToString();

            FilterMateriaSetterListView(_searchString);
        }

        private void ScanForPrefabs()
        {
            SwitchEditMode(EditMode.Native);
            LoadPrefabs((MateriaAtlas)_atlasObjectField.value);

            GenerateListView(_compatibleMateriaSetters);

            _atlasSectionContainer.visible = false;
        }

        private void SelectAtlas()
        {
            _atlasSectionContainer.visible = true;
        }

        private void LoadAtlas()
        {
            SwitchEditMode(EditMode.Atlas);
            _atlas = (MateriaAtlas)_atlasObjectField.value;

            LoadPrefabs(_atlas);

            GenerateListView(_compatibleMateriaSetters);
        }

        private void RemoveAtlasData()
        {
            foreach (var item in _selectedListViewMateriaSetters)
            {
                if (item.MateriaSetterData.MateriaAtlas != null)
                {
                    var itemData = item.MateriaSetterData;

                    item.UnloadAtlas();
                    itemData.MateriaAtlas.AtlasEntries.Remove(item);
                    itemData.MateriaAtlas = null;

                    itemData.AtlasedGridSize = 0;
                    itemData.AtlasedUVRect = new Rect(0f, 0f, 1f, 1f);

                    AssetDatabase.RemoveObjectFromAsset(itemData.AtlasedMesh);
                    itemData.AtlasedMesh = null;
                }
            }
            AssetDatabase.SaveAssets();
        }

        private void LoadPrefabs(MateriaAtlas atlas = null)
        {
            if (atlas == null)
            {
                //ResetAtlasGenerator(true);
                _materiaSetters = AssetUtils.FindAllComponentsInPrefabs<MateriaSetter>();
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

            CheckMateriaSettersCompatibility(_materiaSetters);

            _selectedMateriaSettersValue.text = _materiaSetters.Count.ToString();
        }

        private void SwitchEditMode(EditMode editMode)
        {
            _editMode = editMode;

            /*if (editMode == EditMode.Atlas)
            {
                _removeAtlasData.visible = true;
            }

            if (editMode == EditMode.Native)
            {
                _removeAtlasData.visible = false;
            }*/
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
                        if (!_compatibleMateriaSetters.Contains(ms))
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
                }
                else
                {
                    if (!_incompatibleMateriaSetters.Contains(ms))
                    {
                        _incompatibleMateriaSetters.Add(ms);
                    }
                }
            }
        }

        private void Overwrite()
        {
            if (EditorUtility.DisplayDialog("Overwrite current atlas?", "Are you sure you want to overwrite " + _atlasObjectField.value.name + " with current settings?", "Yes", "No"))
            {
                var kvp = new KeyValuePair<MaterialData, List<MateriaSetter>>(_atlas.MaterialData, _compatibleMateriaSetters);
                AtlasFactory.CreateAtlas(kvp, kvp.Key.Material, AssetDatabase.GetAssetPath(_atlas), _atlas);
            }
        }

        private void SaveAsNew()
        {
            var path = EditorUtility.SaveFilePanelInProject("Choose where to save", "", "asset", "asset");
            if (path.Length != 0)
            {
                var i = 0;
                foreach (var kvp in _groupsList)
                {
                    AtlasFactory.CreateAtlas(kvp, _materialDataMaterialGroups[kvp.Key], path, null);
                    i++;
                }
            }
        }

        private void RegisterCallbacks()
        {
            _materiaSetterSearchToolbarTextField.RegisterCallback<ChangeEvent<string>>(e =>
            {
                FilterMateriaSetterListView(e.newValue);
            });

            RegisterDropAreaCallbacks();
        }

        private void RegisterDropAreaCallbacks()
        {
            _materiaSetterListViewDropArea.RegisterCallback<MouseDownEvent>(evt =>
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData("GameObject", this);
                DragAndDrop.StartDrag("Dragging");
                //DragAndDrop.objectReferences = new Object[] { prefab };
            });

            _materiaSetterListViewDropArea.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            });

            _materiaSetterListViewDropArea.RegisterCallback<DragPerformEvent>(evt =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;

                var objs = new List<GameObject>();

                DragAndDrop.AcceptDrag();

                foreach (GameObject draggedObject in DragAndDrop.objectReferences)
                    if (draggedObject.GetType() == typeof(GameObject))
                        objs.Add((GameObject)draggedObject);

                HandleDraggedInGameObjects(objs);
                GenerateListView(_compatibleMateriaSetters);
                //Action(objs);

            });

            //_materiaSetterListViewDropArea.RegisterCallback<DragEnterEvent>(OnDragEnterEvent);
            //_materiaSetterListViewDropArea.RegisterCallback<DragLeaveEvent>(OnDragLeaveEvent);
            //_materiaSetterListViewDropArea.RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
            //_materiaSetterListViewDropArea.RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
            //_materiaSetterListViewDropArea.RegisterCallback<DragExitedEvent>(OnDragExitedEvent);
        }


        private void HandleDraggedInGameObjects(List<GameObject> gameObjects)
        {
            foreach (var go in gameObjects)
            {
                var materiaSetters = go.GetComponentsInChildren<MateriaSetter>();

                for (int i = 0; i < materiaSetters.Length; i++)
                {
                    if (!_materiaSetters.Contains(materiaSetters[i]))
                    {
                        _materiaSetters.Add(materiaSetters[i]);
                    }
                }

                CheckMateriaSettersCompatibility(materiaSetters);
            }
        }

        protected override void GetProperties()
        {
            _materiaSetterListViewDropArea = root.Q<VisualElement>("MateriaSetterListViewDropArea");

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
            _removeAtlasData = root.Q<Button>("UnlinkAtlasButton");
            _overwriteButton = root.Q<Button>("OverwriteButton");
            _saveAsNewButton = root.Q<Button>("SaveAsNewButton");
        }

        protected override void BindProperties()
        {
            _materiaSetterDataListView.Bind(serializedObject);
        }

        protected override void RegisterButtons()
        {
            _scanProjectButton.clicked += ScanForPrefabs;
            _selectAtlasButton.clicked += SelectAtlas;
            _loadAtlasButton.clicked += LoadAtlas;
            _removeAtlasData.clicked += RemoveAtlasData;
            _overwriteButton.clicked += Overwrite;
            _saveAsNewButton.clicked += SaveAsNew;
        }
    }
}