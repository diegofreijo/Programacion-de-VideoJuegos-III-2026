namespace Clase07.Mvx.Mvp
{
    // Ninguna línea de esta clase importa UnityEngine: solo conoce InventoryModel
    // (dominio) e IInventoryView (interfaz pasiva). Por eso es la primera de las tres
    // variantes testeable con un fake, sin levantar Unity.
    public class InventoryPresenter
    {
        private readonly InventoryModel _model;
        private readonly IInventoryView _view;

        public InventoryPresenter(InventoryModel model, IInventoryView view)
        {
            _model = model;
            _view = view;
            _view.AddRequested += OnAddRequested;
            _view.RemoveRequested += OnRemoveRequested;
            _model.Changed += Render;
            Render();
        }

        private void OnAddRequested(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _model.AddItem(name);
        }

        private void OnRemoveRequested(string name) => _model.RemoveItem(name);

        private void Render() => _view.ShowItems(_model.Items);
    }
}
