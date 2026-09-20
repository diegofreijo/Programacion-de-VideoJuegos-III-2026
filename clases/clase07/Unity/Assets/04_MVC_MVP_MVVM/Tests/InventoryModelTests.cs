using NUnit.Framework;
using System.Linq;
using Clase07.Mvx.Shared;

namespace Clase07.Mvx.Tests
{
    public class InventoryModelTests
    {
        [Test]
        public void AddItem_NewName_AddsWithQuantityOne()
        {
            var model = new InventoryModel();
            model.AddItem("Potion");
            Assert.AreEqual(1, model.Items.Single(i => i.Name == "Potion").Quantity);
        }

        [Test]
        public void AddItem_ExistingName_IncrementsQuantity()
        {
            var model = new InventoryModel();
            model.AddItem("Potion");
            model.AddItem("Potion");
            Assert.AreEqual(2, model.Items.Single(i => i.Name == "Potion").Quantity);
        }

        [Test]
        public void AddItem_EmptyName_Throws()
        {
            var model = new InventoryModel();
            Assert.Throws<System.ArgumentException>(() => model.AddItem(""));
        }

        [Test]
        public void RemoveItem_DecrementsQuantity_AndRemovesAtZero()
        {
            var model = new InventoryModel();
            model.AddItem("Potion");
            model.AddItem("Potion");

            model.RemoveItem("Potion");
            Assert.AreEqual(1, model.Items.Single(i => i.Name == "Potion").Quantity);

            model.RemoveItem("Potion");
            Assert.IsEmpty(model.Items);
        }

        [Test]
        public void RemoveItem_UnknownName_IsNoOp()
        {
            var model = new InventoryModel();
            Assert.DoesNotThrow(() => model.RemoveItem("Nothing"));
            Assert.IsEmpty(model.Items);
        }

        [Test]
        public void Changed_FiresOnAddAndRemove()
        {
            var model = new InventoryModel();
            var fireCount = 0;
            model.Changed += () => fireCount++;

            model.AddItem("Potion");
            model.RemoveItem("Potion");

            Assert.AreEqual(2, fireCount);
        }
    }
}
