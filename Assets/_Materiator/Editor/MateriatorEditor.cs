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
            root = new VisualElement();
            tree = Resources.Load<VisualTreeAsset>(typeof(T).Name);
            tree.CloneTree(root);
        }
    }
}
