using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Clase07.Mvx.Mvp
{
    public class InventoryMvpView : MonoBehaviour, IInventoryView
    {
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Transform _listContent;
        [SerializeField] private InventoryItemRowView _rowPrefab;

        public event Action<string> AddRequested;
        public event Action<string> RemoveRequested;

        private void Awake()
        {
            new InventoryPresenter(new InventoryModel(), this);
        }

        public void OnAddClicked()
        {
            AddRequested?.Invoke(_nameInput.text);
            _nameInput.text = string.Empty;
        }

        public void ShowItems(IReadOnlyList<InventoryItem> items)
        {
            for (var i = _listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_listContent.GetChild(i).gameObject);
            }

            foreach (var item in items)
            {
                var row = Instantiate(_rowPrefab, _listContent);
                row.SetLabel($"{item.Name} x{item.Quantity}");
                var itemName = item.Name;
                row.SetRemoveAction(() => RemoveRequested?.Invoke(itemName));
            }
        }
    }
}
