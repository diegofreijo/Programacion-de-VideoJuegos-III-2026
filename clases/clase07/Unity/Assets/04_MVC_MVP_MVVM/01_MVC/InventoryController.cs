using UnityEngine;
using TMPro;
using Clase07.Mvx.Shared;

namespace Clase07.Mvx.Mvc
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Transform _listContent;
        [SerializeField] private InventoryItemRowView _rowPrefab;

        private readonly InventoryModel _model = new InventoryModel();

        private void Awake()
        {
            _model.Changed += RenderList;
            RenderList();
        }

        public void OnAddClicked()
        {
            if (string.IsNullOrWhiteSpace(_nameInput.text)) return;
            _model.AddItem(_nameInput.text);
            _nameInput.text = string.Empty;
        }

        private void RenderList()
        {
            for (var i = _listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_listContent.GetChild(i).gameObject);
            }

            foreach (var item in _model.Items)
            {
                var row = Instantiate(_rowPrefab, _listContent);
                row.SetLabel($"{item.Name} x{item.Quantity}");
                var itemName = item.Name;
                row.SetRemoveAction(() => _model.RemoveItem(itemName));
            }
        }
    }
}
