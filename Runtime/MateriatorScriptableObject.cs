using UnityEngine;

namespace Materiator
{
    public abstract class MateriatorScriptableObject : ScriptableObject
    {
        [SerializeField, TextArea(3, 0)]
        private string _description;
    }
}