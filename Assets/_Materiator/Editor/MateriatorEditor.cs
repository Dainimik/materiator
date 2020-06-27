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
            root.styleSheets.Add(Resources.Load<StyleSheet>("Materiator"));
            tree.CloneTree(root);

            GetProperties();
            BindProperties();
            RegisterButtons();
            
        }

        protected virtual void GetProperties() { }

        protected virtual void BindProperties() { }

        protected virtual void RegisterButtons() { }
    }
}
