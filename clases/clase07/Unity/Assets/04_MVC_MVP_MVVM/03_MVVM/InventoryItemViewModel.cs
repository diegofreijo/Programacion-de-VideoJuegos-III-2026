using System;

namespace Clase07.Mvx.Mvvm
{
    public class InventoryItemViewModel
    {
        public string Name { get; }
        public int Quantity { get; }
        public RelayCommand RemoveCommand { get; }

        public InventoryItemViewModel(string name, int quantity, Action onRemove)
        {
            Name = name;
            Quantity = quantity;
            RemoveCommand = new RelayCommand(onRemove);
        }
    }
}
