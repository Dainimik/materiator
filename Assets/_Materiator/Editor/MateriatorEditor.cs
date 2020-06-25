using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Materiator
{
    public abstract class MateriatorEditor : Editor
    {
        protected VisualElement root;
        protected VisualTreeAsset tree;

        protected void InitializeEditor<T>()
        {
            if (root == null)
                root = new VisualElement();
            else
                root.Clear();

            tree = Resources.Load<VisualTreeAsset>(typeof(T).Name);
            tree.CloneTree(root);
        }
    }
}
