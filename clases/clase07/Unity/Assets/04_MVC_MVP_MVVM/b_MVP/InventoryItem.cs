namespace Clase07.Mvx.Mvp
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
