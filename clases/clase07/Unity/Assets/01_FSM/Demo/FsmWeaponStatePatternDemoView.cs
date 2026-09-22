using UnityEngine;
using TMPro;
using Clase07.FSM.Small.StatePattern;

namespace Clase07.FSM.Demo
{
    public class FsmWeaponStatePatternDemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _stateLabel;

        private WeaponStatePatternController _weapon;

        private void Awake() => _weapon = new WeaponStatePatternController();

        public void OnFireClicked() => _weapon.PressTrigger();

        private void Update()
        {
            _weapon.Tick(Time.deltaTime);
            _stateLabel.text = $"[StatePattern] ammo: {_weapon.Context.AmmoInMagazine}";
        }
    }
}
