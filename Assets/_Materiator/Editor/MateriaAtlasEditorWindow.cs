using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Materiator
{
    public class MateriaAtlasEditorWindow : MateriatorEditorWindow
    {
        private ListView _materiaSetterDataListView;

        private Button _scanProjectButton;
        private Button _overwriteButton;
        private Button _saveAsNewButton;

        private List<MateriaSetter> _materiaSetters;
        private List<MateriaSetterData> _materiaSetterDataList;

        [MenuItem("Tools/Materiator/Atlas Editor")]

        public static void OpenWindow()
        {
            GetWindow<MateriaAtlasEditorWindow>("Atlas Editor");
        }

        private void OnEnable()
        {
            InitializeEditorWindow<MateriaAtlasEditorWindow>();
            serializedObject = new SerializedObject(this);
        }

        private void GenerateListView()
        {
            // The "makeItem" function will be called as needed
            // when the ListView needs more items to render
            Func<VisualElement> makeItem = () => new Label();

            // As the user scrolls through the list, the ListView object
            // will recycle elements created by the "makeItem"
            // and invoke the "bindItem" callback to associate
            // the element with the matching data item (specified as an index in the list)
            Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = (i + 1) + ". " + _materiaSetters[i].name;

            _materiaSetterDataListView.makeItem = makeItem;
            _materiaSetterDataListView.bindItem = bindItem;
            _materiaSetterDataListView.itemsSource = _materiaSetters;

            // Callback invoked when the user double clicks an item
            _materiaSetterDataListView.onItemsChosen += Debug.Log;

            // Callback invoked when the user changes the selection inside the ListView
            _materiaSetterDataListView.onSelectionChange += Debug.Log;
        }

        private void Scan()
        {
            LoadPrefabs();

            GenerateListView();
        }

        private void LoadPrefabs(MateriaAtlas atlas = null)
        {
            if (atlas == null)
            {
                //ResetAtlasGenerator(true);
                _materiaSetters = AssetUtils.FindAllComponentsInPrefabs<MateriaSetter>();
                //CheckMateriaSettersCompatibility(_materiaSetters);
            }
            else
            {
                //ResetAtlasGenerator(false);
                foreach (var msd in atlas.MateriaSetterDatas)
                {
                    if (msd == null) return;

                    var go = (GameObject)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(msd));
                    var colorSetters = go.GetComponentsInChildren<MateriaSetter>();
                    //CheckMateriaSettersCompatibility(colorSetters);
                }
            }
        }

        private void CheckMateriaSettersCompatibility(ICollection<MateriaSetter> colorSetters)
        {
            /*foreach (var cs in colorSetters)
            {
                var groupMembers = new List<MateriaSetter>();
                //-----TODO: Have an option to create atlas without having a preset set?
                if (AtlasFactory.CheckColorSetterCompatibility(cs))
                {
                    if (PrefabUtility.IsPartOfPrefabAsset(cs)
                    && !PrefabUtility.IsPartOfPrefabInstance(cs) || PrefabUtility.IsPartOfVariantPrefab(cs))
                    {
                        _compatibleColorSetters.Add(cs);

                        if (!_groupsList.ContainsKey(cs.ColorPreset.ShaderData))
                        {
                            _groupsList.Add(cs.ColorPreset.ShaderData, groupMembers);
                        }
                        _groupsList[cs.ColorPreset.ShaderData].Add(cs);

                        if (!_shaderMaterialGroups.ContainsKey(cs.ColorPreset.ShaderData))
                            _shaderMaterialGroups.Add(cs.ColorPreset.ShaderData, cs.ColorPreset.Material);
                    }
                }
                else
                    _incompatibleColorSetters.Add(cs);
            }

            _groupsCount = _groupsList.Count;*/
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
                //foreach (var kvp in _groupsList)
                //{
                    //AtlasFactory.CreateAtlas(kvp, _shaderMaterialGroups[kvp.Key], path, _saveAsNewPrefabs.value, _newPrefabSuffix);
                    AtlasFactory.CreateAtlas(new KeyValuePair<ShaderData, List<MateriaSetter>>(Utils.Settings.DefaultShaderData, _materiaSetters), new Material(Utils.Settings.DefaultShaderData.Shader), path, true, "_newPrefabSuffix");
                //i++;
                //}
            }
        }

        protected override void GetProperties()
        {
            _materiaSetterDataListView = root.Q<ListView>("MateriaSetterDataListView");

            _scanProjectButton = root.Q<Button>("ScanProjectButton");
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
            _overwriteButton.clicked += Overwrite;
            _saveAsNewButton.clicked += SaveAsNew;
        }
    }
}