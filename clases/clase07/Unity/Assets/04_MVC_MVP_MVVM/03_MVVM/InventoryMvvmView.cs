using UnityEngine;
using UnityEngine.UI;
using Clase07.Mvx.Shared;
using Clase07.Shared.UI;

namespace Clase07.Mvx.Mvvm
{
    public class InventoryMvvmView : MonoBehaviour
    {
        private InputField _nameInput;
        private Transform _listRoot;
        private InventoryViewModel _viewModel;

        private void Awake() => Build();

        public void Build()
        {
            _viewModel = new InventoryViewModel(new InventoryModel());

            var canvas = DemoUiFactory.CreateCanvas();
            _nameInput = DemoUiFactory.CreateInputField(canvas.transform, new Vector2(-100, 200));
            _nameInput.onValueChanged.AddListener(value => _viewModel.PendingName = value);
            var addButton = DemoUiFactory.CreateButton(canvas.transform, "Add", new Vector2(120, 200));
            addButton.onClick.AddListener(() =>
            {
                _viewModel.AddCommand.Execute();
                _nameInput.text = string.Empty;
            });

            _listRoot = new GameObject("List").transform;
            _listRoot.SetParent(canvas.transform, false);

            _viewModel.Items.CollectionChanged += (_, __) => Render();
            Render();
        }

        private void Render()
        {
            for (var i = _listRoot.childCount - 1; i >= 0; i--) Destroy(_listRoot.GetChild(i).gameObject);

            var y = 150;
            foreach (var itemVm in _viewModel.Items)
            {
                DemoUiFactory.CreateLabel(_listRoot, $"{itemVm.Name} x{itemVm.Quantity}", new Vector2(-100, y));
                var removeButton = DemoUiFactory.CreateButton(_listRoot, "Remove", new Vector2(120, y));
                removeButton.onClick.AddListener(itemVm.RemoveCommand.Execute);
                y -= 40;
            }
        }
    }
}
