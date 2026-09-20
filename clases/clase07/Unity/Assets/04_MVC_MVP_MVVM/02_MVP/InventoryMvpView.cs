using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Clase07.Mvx.Shared;
using Clase07.Shared.UI;

namespace Clase07.Mvx.Mvp
{
    public class InventoryMvpView : MonoBehaviour, IInventoryView
    {
        public event Action<string> AddRequested;
        public event Action<string> RemoveRequested;

        private InputField _nameInput;
        private Transform _listRoot;

        private void Awake() => Build();

        public void Build()
        {
            var canvas = DemoUiFactory.CreateCanvas();
            _nameInput = DemoUiFactory.CreateInputField(canvas.transform, new Vector2(-100, 200));
            var addButton = DemoUiFactory.CreateButton(canvas.transform, "Add", new Vector2(120, 200));
            addButton.onClick.AddListener(() =>
            {
                AddRequested?.Invoke(_nameInput.text);
                _nameInput.text = string.Empty;
            });

            _listRoot = new GameObject("List").transform;
            _listRoot.SetParent(canvas.transform, false);

            new InventoryPresenter(new InventoryModel(), this);
        }

        public void ShowItems(IReadOnlyList<InventoryItem> items)
        {
            for (var i = _listRoot.childCount - 1; i >= 0; i--) Destroy(_listRoot.GetChild(i).gameObject);

            var y = 150;
            foreach (var item in items)
            {
                DemoUiFactory.CreateLabel(_listRoot, $"{item.Name} x{item.Quantity}", new Vector2(-100, y));
                var removeButton = DemoUiFactory.CreateButton(_listRoot, "Remove", new Vector2(120, y));
                var itemName = item.Name;
                removeButton.onClick.AddListener(() => RemoveRequested?.Invoke(itemName));
                y -= 40;
            }
        }
    }
}
