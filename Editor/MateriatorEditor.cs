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
            RegisterCallbacks();

            SetUpView();
        }

        protected abstract void SetUpView();

        protected abstract void GetProperties();

        protected abstract void BindProperties();

        protected abstract void RegisterCallbacks();
    }
}
