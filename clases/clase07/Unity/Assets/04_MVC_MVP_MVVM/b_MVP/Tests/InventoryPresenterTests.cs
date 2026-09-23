using System;
using System.Collections.Generic;
using NUnit.Framework;
using Clase07.Mvx.Mvp;

namespace Clase07.Mvx.Mvp.Tests
{
    public class FakeInventoryView : IInventoryView
    {
        public event Action<string> AddRequested;
        public event Action<string> RemoveRequested;
        public IReadOnlyList<InventoryItem> LastShown { get; private set; }

        public void ShowItems(IReadOnlyList<InventoryItem> items) => LastShown = items;
        public void RaiseAddRequested(string name) => AddRequested?.Invoke(name);
        public void RaiseRemoveRequested(string name) => RemoveRequested?.Invoke(name);
    }

    public class InventoryPresenterTests
    {
        [Test]
        public void Construction_RendersEmptyInitialState()
        {
            var view = new FakeInventoryView();
            new InventoryPresenter(new InventoryModel(), view);

            Assert.IsEmpty(view.LastShown);
        }

        [Test]
        public void AddRequested_AddsItemAndRerenders()
        {
            var view = new FakeInventoryView();
            new InventoryPresenter(new InventoryModel(), view);

            view.RaiseAddRequested("Potion");

            Assert.AreEqual(1, view.LastShown.Count);
            Assert.AreEqual("Potion", view.LastShown[0].Name);
        }

        [Test]
        public void RemoveRequested_RemovesItemAndRerenders()
        {
            var view = new FakeInventoryView();
            new InventoryPresenter(new InventoryModel(), view);
            view.RaiseAddRequested("Potion");

            view.RaiseRemoveRequested("Potion");

            Assert.IsEmpty(view.LastShown);
        }

        [Test]
        public void AddRequested_WithEmptyName_DoesNotAddItem()
        {
            var view = new FakeInventoryView();
            new InventoryPresenter(new InventoryModel(), view);

            view.RaiseAddRequested("   ");

            Assert.IsEmpty(view.LastShown);
        }
    }
}
