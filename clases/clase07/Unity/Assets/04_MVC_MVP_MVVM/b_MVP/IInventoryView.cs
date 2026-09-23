using System;
using System.Collections.Generic;

namespace Clase07.Mvx.Mvp
{
    // El presenter (ver InventoryPresenter) habla únicamente con esta interfaz —
    // nunca con InventoryMvpView ni con ningún tipo de UnityEngine. Por eso se puede
    // testear con un fake en C# puro, sin abrir Unity (ver Tests/InventoryPresenterTests.cs),
    // a diferencia de a_MVC donde la vista y la lógica están mezcladas.
    public interface IInventoryView
    {
        event Action<string> AddRequested;
        event Action<string> RemoveRequested;
        void ShowItems(IReadOnlyList<InventoryItem> items);
    }
}
