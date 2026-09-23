using System.Linq;
using NUnit.Framework;
using Clase07.Mvx.Mvvm;

namespace Clase07.Mvx.Mvvm.Tests
{
    public class InventoryViewModelTests
    {
        [Test]
        public void AddCommand_CanExecute_FalseWhenPendingNameEmpty()
        {
            var vm = new InventoryViewModel(new InventoryModel());
            vm.PendingName = "";
            Assert.IsFalse(vm.AddCommand.CanExecute());
        }

        [Test]
        public void AddCommand_Execute_AddsItemToObservableCollection()
        {
            var vm = new InventoryViewModel(new InventoryModel());
            vm.PendingName = "Potion";

            vm.AddCommand.Execute();

            Assert.AreEqual(1, vm.Items.Count);
            Assert.AreEqual("Potion", vm.Items[0].Name);
        }

        [Test]
        public void RemoveCommand_Execute_RemovesItemFromCollection()
        {
            var vm = new InventoryViewModel(new InventoryModel());
            vm.PendingName = "Potion";
            vm.AddCommand.Execute();

            vm.Items.Single().RemoveCommand.Execute();

            Assert.IsEmpty(vm.Items);
        }
    }
}
