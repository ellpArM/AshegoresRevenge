using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(fileName = "ItemParameterSO", menuName = "Scriptable Objects/ItemParameterSO")]
    public class ItemParameterSO : ScriptableObject
    {
        [field: SerializeField]
        public string parameterName { get; private set; }
    }
}