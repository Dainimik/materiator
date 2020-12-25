using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaAtlas))]
    public class MateriaAtlasEditor : MateriatorEditor
    {
        public MateriaSection MateriaSection;

        public MateriaAtlas MateriaAtlas;

        public IMGUIContainer IMGUIContainer;

        private Button _generateAtlasButton;

        private void OnEnable()
        {
            MateriaAtlas = (MateriaAtlas)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaAtlas>();

            IMGUIContainer defaultInspector = new IMGUIContainer(() => IMGUI());
            IMGUIContainer.Add(defaultInspector);

            MateriaSection = new MateriaSection(this, "MateriaSlots");

            return root;
        }

        private void IMGUI()
        {
            base.DrawDefaultInspector();
        }

        private void GenerateAtlas()
        {
            MateriaAtlas.AtlasItems.Clear();

            var r = new Rect(0f, 0f, 1f, 1f);

            foreach (var materiaSlot in MateriaAtlas.MateriaSlots)
            {
                var textures = InitializeTextures();
                var rect = new Rect(0f, 0f, textures.Size.x, textures.Size.y);

                textures.UpdateColors(r, materiaSlot.Materia.Properties);

                MateriaAtlas.AtlasItems.Add(new MateriaAtlasItem(materiaSlot, textures));
            }

            MateriaAtlas.Textures = InitializeTextures();
            Rect[] atlasRects;

            foreach (var texture in MateriaAtlas.Textures.Texs.ToArray())
            {
                var newTex = new Texture2D(8192, 8192);
                newTex.filterMode = SystemData.Settings.FilterMode;

                var atlasItemTextures = MateriaAtlas.AtlasItems.SelectMany(item => item.Textures.Texs.Where(tex => tex.Key == texture.Key)).Select(pairs => pairs.Value).ToArray();

                atlasRects = newTex.PackTextures(atlasItemTextures, 0, 8192, false);

                for (int i = 0; i < atlasRects.Length; i++)
                {
                    MateriaAtlas.AtlasItems[i].Rect = atlasRects[i];
                }

                MateriaAtlas.Textures.Texs[texture.Key] = newTex;
            }

            MateriaAtlas.Textures.AddTexturesToAsset(MateriaAtlas);

            SetTextures();

            AssetDatabase.SaveAssets();
        }

        private Textures InitializeTextures()
        {
            var shaderProps = MateriaAtlas.MaterialData.ShaderData.MateriatorShaderProperties;

            var textures = new Textures();

            var gridSize = SystemData.Settings.DefaultGridSize;

            textures.RemoveTextures(shaderProps, gridSize.x, gridSize.y);
            textures.CreateTextures(shaderProps, gridSize.x, gridSize.y);

            return textures;
        }

        public void SetTextures()
        {
            if (MateriaAtlas.Material == null) return;

            MateriaAtlas.Textures.SetTexturesToMaterial(MateriaAtlas.Material);
        }

        protected override void GetProperties()
        {

        }

        protected override void BindProperties()
        {
            IMGUIContainer = root.Q<IMGUIContainer>("IMGUIContainer");

            _generateAtlasButton = root.Q<Button>("GenerateAtlasButton");
        }

        protected override void SetUpView()
        {
            
        }

        protected override void RegisterCallbacks()
        {
            _generateAtlasButton.clicked += GenerateAtlas;
        }
    }
}