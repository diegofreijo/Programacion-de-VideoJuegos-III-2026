using System;
using System.Collections.Generic;
using Clase07.Mvx.Shared;

namespace Clase07.Mvx.Mvp
{
    public interface IInventoryView
    {
        event Action<string> AddRequested;
        event Action<string> RemoveRequested;
        void ShowItems(IReadOnlyList<InventoryItem> items);
    }
}
