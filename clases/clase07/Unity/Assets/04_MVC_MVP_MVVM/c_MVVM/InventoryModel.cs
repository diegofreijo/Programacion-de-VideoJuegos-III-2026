using System;
using System.Collections.Generic;
using System.Linq;

namespace Clase07.Mvx.Mvvm
{
    public class InventoryModel
    {
        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        public IReadOnlyList<InventoryItem> Items => _items;
        public event Action Changed;

        public void AddItem(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("El nombre no puede estar vacío", nameof(name));
            }

            var existing = _items.FirstOrDefault(i => i.Name == name);
            if (existing != null) existing.Quantity++;
            else _items.Add(new InventoryItem(name, 1));

            Changed?.Invoke();
        }

        public void RemoveItem(string name)
        {
            var existing = _items.FirstOrDefault(i => i.Name == name);
            if (existing == null) return;

            existing.Quantity--;
            if (existing.Quantity <= 0) _items.Remove(existing);

            Changed?.Invoke();
        }
    }
}
