using UnityEngine;
using TMPro;

namespace Clase07.Mvx.Mvvm
{
    public class InventoryMvvmView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Transform _listContent;
        [SerializeField] private InventoryItemRowView _rowPrefab;

        private InventoryViewModel _viewModel;

        private void Awake()
        {
            _viewModel = new InventoryViewModel(new InventoryModel());
            _nameInput.onValueChanged.AddListener(value => _viewModel.PendingName = value);
            // La vista nunca llama a _model.AddItem/RemoveItem directamente: solo
            // escribe PendingName y ejecuta AddCommand/RemoveCommand (ver OnAddClicked
            // y SetRemoveAction más abajo), y solo redibuja cuando Items.CollectionChanged
            // avisa. Toda la lógica de cuándo se puede agregar o quitar un ítem vive en
            // InventoryViewModel/RelayCommand, no acá — a diferencia de a_MVC, que llama
            // al modelo directamente.
            _viewModel.Items.CollectionChanged += (_, __) => Render();
            Render();
        }

        public void OnAddClicked()
        {
            _viewModel.AddCommand.Execute();
            _nameInput.text = string.Empty;
        }

        private void Render()
        {
            for (var i = _listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_listContent.GetChild(i).gameObject);
            }

            foreach (var itemVm in _viewModel.Items)
            {
                var row = Instantiate(_rowPrefab, _listContent);
                row.SetLabel($"{itemVm.Name} x{itemVm.Quantity}");
                row.SetRemoveAction(itemVm.RemoveCommand.Execute);
            }
        }
    }
}
