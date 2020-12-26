using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine.UIElements;

namespace Materiator
{
    public class SystemChecker
    {
        public delegate void ContextAction(MateriaSetterEditor editor);
        public ContextAction _contextAction;
        public static bool CheckAllSystems(MateriaSetterEditor editor)
        {
            var materiaSetter = editor.MateriaSetter;
            var root = editor.Root;

            materiaSetter.GetMeshReferences();

            if ((materiaSetter.MeshRenderer != null && materiaSetter.SkinnedMeshRenderer != null) || (materiaSetter.MeshFilter != null && materiaSetter.SkinnedMeshRenderer != null))
            {
                return ErrorMessage(editor, "Please use either only a SKINNED MESH RENDERER component alone or a MESH FILTER + MESH RENDERER component combo.");
            }
            else if (materiaSetter.Renderer == null && materiaSetter.MeshFilter == null)
            {
                return ErrorMessage(
                    editor,
                    "Please first add a MESH FILTER or a SKINNED MESH RENDERER component to this Game Object.",
                    new Dictionary<ContextAction, string>
                    {
                        [ContextActions.AddMeshFilter] = "Add Mesh Filter",
                        [ContextActions.AddSkinnedMeshRenderer] = "Add Skinned Mesh Renderer"
                    });
            }
            else if (materiaSetter.MeshRenderer != null && materiaSetter.MeshFilter == null)
            {
                return ErrorMessage(
                    editor,
                    "Please first add a MESH FILTER component to this Game Object.",
                    new Dictionary<ContextAction, string>
                    {
                        [ContextActions.AddMeshFilter] = "Add Mesh Filter"
                    });
            }
            else if (materiaSetter.SkinnedMeshRenderer == null && materiaSetter.MeshFilter.sharedMesh == null)
            {
                return ErrorMessage(
                    editor,
                    "Please add a MESH and hit the Retry button.",
                    new Dictionary<ContextAction, string> { [ContextActions.Retry] = "Retry" }
                    );
            }
            else if (materiaSetter.SkinnedMeshRenderer == null && materiaSetter.MeshRenderer == null && materiaSetter.MeshFilter == null)
            {
                return ErrorMessage(
                    editor,
                    "Please first add a MESH RENDERER or a SKINNED MESH RENDERER component to this Game Object.",
                    new Dictionary<ContextAction, string>
                    {
                        [ContextActions.AddMeshRenderer] = "Add Mesh Renderer",
                        [ContextActions.AddSkinnedMeshRenderer] = "Add Skinned Mesh Renderer"
                    });
            }
            else if (materiaSetter.SkinnedMeshRenderer == null && materiaSetter.MeshRenderer == null && materiaSetter.MeshFilter != null)
            {
                return ErrorMessage(
                    editor,
                    "Please first add a MESH RENDERER component to this Game Object.",
                    new Dictionary<ContextAction, string>
                    {
                        [ContextActions.AddMeshRenderer] = "Add Mesh Renderer"
                    });
            }
            else if (materiaSetter.SkinnedMeshRenderer != null && materiaSetter.SkinnedMeshRenderer.sharedMesh == null)
            {
                return ErrorMessage(
                    editor,
                    "Please first add a MESH to a SKINNED MESH RENDERER component and hit the Retry Button.",
                    new Dictionary<ContextAction, string>
                    {
                        [ContextActions.Retry] = "Retry"
                    });
            }
            else if (PrefabUtility.IsPartOfPrefabAsset(materiaSetter) && !PrefabStageUtility.GetCurrentPrefabStage()) // This is here because switching edit mode in project view prefab causes bugs
            {
                return ErrorMessage(
                    editor,
                    "Please Open Prefab to edit Materia Setter.",
                    new Dictionary<ContextAction, string>
                    {
                        [ContextActions.OpenPrefab] = "Open Prefab"
                    });
            }
            else
            {
                return true;
            }
        }

        public static bool ErrorMessage(MateriaSetterEditor editor, string text, Dictionary<ContextAction, string> actionContent = null)
        {
            var root = editor.Root;

            root.Clear();
            var warning = new HelpBox(text, HelpBoxMessageType.Warning);
            root.Add(warning);

            if (actionContent != null)
            {
                foreach (var entry in actionContent)
                {
                    var button = new Button(() => entry.Key(editor));
                    button.text = entry.Value;
                    root.Add(button);
                }
            }

            return false;
        }
    }
}