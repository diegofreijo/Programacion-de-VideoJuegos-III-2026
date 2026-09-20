using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Clase07.Mvx.Shared;
using Clase07.Shared.UI;

namespace Clase07.Mvx.Mvc
{
    public class InventoryController : MonoBehaviour
    {
        private readonly InventoryModel _model = new InventoryModel();
        private InputField _nameInput;
        private Transform _listRoot;
        private readonly List<GameObject> _rows = new List<GameObject>();

        private void Awake() => Build();

        public void Build()
        {
            var canvas = DemoUiFactory.CreateCanvas();
            _nameInput = DemoUiFactory.CreateInputField(canvas.transform, new Vector2(-100, 200));
            var addButton = DemoUiFactory.CreateButton(canvas.transform, "Add", new Vector2(120, 200));
            addButton.onClick.AddListener(OnAddClicked);

            _listRoot = new GameObject("List").transform;
            _listRoot.SetParent(canvas.transform, false);

            _model.Changed += RenderList;
        }

        private void OnAddClicked()
        {
            if (string.IsNullOrWhiteSpace(_nameInput.text)) return;
            _model.AddItem(_nameInput.text);
            _nameInput.text = string.Empty;
        }

        private void RenderList()
        {
            foreach (var row in _rows) Destroy(row);
            _rows.Clear();

            var y = 150;
            foreach (var item in _model.Items)
            {
                DemoUiFactory.CreateLabel(_listRoot, $"{item.Name} x{item.Quantity}", new Vector2(-100, y));
                var removeButton = DemoUiFactory.CreateButton(_listRoot, "Remove", new Vector2(120, y));
                var itemName = item.Name;
                removeButton.onClick.AddListener(() => _model.RemoveItem(itemName));
                y -= 40;
            }
        }
    }
}
