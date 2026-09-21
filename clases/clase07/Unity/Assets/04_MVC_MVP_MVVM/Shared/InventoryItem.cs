namespace Clase07.Mvx.Shared
{
    public class InventoryItem
    {
        public string Name { get; }
        public int Quantity { get; set; }

        public InventoryItem(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }
    }
}
