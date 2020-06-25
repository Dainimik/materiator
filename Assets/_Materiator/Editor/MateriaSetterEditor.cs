using UnityEditor;
using UnityEngine.UIElements;

namespace Materiator
{
    [CustomEditor(typeof(MateriaSetter))]
    public class MateriaSetterEditor : MateriatorEditor
    {
        private MateriaSetter _materiaSetter;

        private Button _reloadButton;

        private void OnEnable()
        {
            _materiaSetter = (MateriaSetter)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            InitializeEditor<MateriaSetter>();

            Initialize();

            return root;
        }

        private void Initialize()
        {
            _materiaSetter.GetMeshReferences();

            /*if (!CheckAllSystems())
            {
                return;
            }*/

            SetUpButtons();
            _materiaSetter.Initialize();
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
