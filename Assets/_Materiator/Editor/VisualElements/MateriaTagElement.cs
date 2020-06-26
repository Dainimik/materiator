using UnityEngine;
using UnityEngine.UIElements;

public class MateriaTagElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<MateriaTagElement> { }

    public MateriaTagElement()
    {
        var tree = Resources.Load<VisualTreeAsset>("VisualElements/MateriaTagElement");
        var ui = tree.CloneTree();

        Add(ui);
    }
}
