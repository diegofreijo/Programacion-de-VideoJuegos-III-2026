using UnityEngine;
using TMPro;
using Clase07.FSM.Small.Baseline;

namespace Clase07.FSM.Demo
{
    public class FsmWeaponBaselineDemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _stateLabel;

        private WeaponBaseline _weapon;

        private void Awake() => _weapon = new WeaponBaseline();

        public void OnFireClicked() => _weapon.PressTrigger();

        private void Update()
        {
            _weapon.Tick(Time.deltaTime);
            _stateLabel.text = $"[Baseline] {_weapon.State} (ammo: {_weapon.AmmoInMagazine})";
        }
    }
}
