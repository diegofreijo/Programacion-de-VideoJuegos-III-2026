using UnityEngine;
using TMPro;

namespace Clase07.Mvx.Mvc
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private Transform _listContent;
        [SerializeField] private InventoryItemRowView _rowPrefab;

        private readonly InventoryModel _model = new InventoryModel();

        private void Awake()
        {
            _model.Changed += RenderList;
            RenderList();
        }

        public void OnAddClicked()
        {
            if (string.IsNullOrWhiteSpace(_nameInput.text)) return;
            _model.AddItem(_nameInput.text);
            _nameInput.text = string.Empty;
        }

        // A propósito el controller toca directamente los elementos concretos de la
        // vista (instancia/destruye filas acá adentro) — no hay una interfaz de vista
        // de por medio. Es lo que hace que este patrón sea difícil de testear sin
        // Unity: no existe una "lógica de presentación" separable de la manipulación
        // de GameObjects (comparar con b_MVP/IInventoryView y c_MVVM).
        private void RenderList()
        {
            for (var i = _listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_listContent.GetChild(i).gameObject);
            }

            foreach (var item in _model.Items)
            {
                var row = Instantiate(_rowPrefab, _listContent);
                row.SetLabel($"{item.Name} x{item.Quantity}");
                var itemName = item.Name;
                row.SetRemoveAction(() => _model.RemoveItem(itemName));
            }
        }
    }
}
