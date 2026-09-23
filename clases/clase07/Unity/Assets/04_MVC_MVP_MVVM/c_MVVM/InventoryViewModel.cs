using System.Collections.ObjectModel;

namespace Clase07.Mvx.Mvvm
{
    public class InventoryViewModel
    {
        private readonly InventoryModel _model;
        public ObservableCollection<InventoryItemViewModel> Items { get; } = new ObservableCollection<InventoryItemViewModel>();
        public string PendingName { get; set; } = string.Empty;
        public RelayCommand AddCommand { get; }

        public InventoryViewModel(InventoryModel model)
        {
            _model = model;
            AddCommand = new RelayCommand(
                () => _model.AddItem(PendingName),
                () => !string.IsNullOrWhiteSpace(PendingName));
            _model.Changed += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            Items.Clear();
            foreach (var item in _model.Items)
            {
                var name = item.Name;
                Items.Add(new InventoryItemViewModel(name, item.Quantity, () => _model.RemoveItem(name)));
            }
        }
    }
}
