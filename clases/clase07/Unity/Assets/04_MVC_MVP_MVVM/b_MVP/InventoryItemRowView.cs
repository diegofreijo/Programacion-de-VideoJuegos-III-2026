using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Clase07.Mvx.Mvp
{
    public class InventoryItemRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _removeButton;

        public void SetLabel(string text) => _label.text = text;

        public void SetRemoveAction(UnityEngine.Events.UnityAction onRemove)
        {
            _removeButton.onClick.RemoveAllListeners();
            _removeButton.onClick.AddListener(onRemove);
        }
    }
}
