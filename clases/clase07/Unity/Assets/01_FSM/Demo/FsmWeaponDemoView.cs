using UnityEngine;
using UnityEngine.UI;
using Clase07.FSM.Small.Baseline;
using Clase07.FSM.Small.StatePattern;
using Clase07.Shared.UI;

namespace Clase07.FSM.Demo
{
    public class FsmWeaponDemoView : MonoBehaviour
    {
        private WeaponBaseline _baseline;
        private WeaponStatePatternController _statePattern;
        private Text _baselineLabel;
        private Text _statePatternLabel;

        private void Awake() => Build();

        public void Build()
        {
            _baseline = new WeaponBaseline();
            _statePattern = new WeaponStatePatternController();

            var canvas = DemoUiFactory.CreateCanvas("WeaponCanvas");

            var fireBaseline = DemoUiFactory.CreateButton(canvas.transform, "Fire (Baseline)", new Vector2(-150, 260));
            fireBaseline.onClick.AddListener(() => _baseline.PressTrigger());
            _baselineLabel = DemoUiFactory.CreateLabel(canvas.transform, string.Empty, new Vector2(-150, 220));

            var fireStatePattern = DemoUiFactory.CreateButton(canvas.transform, "Fire (State Pattern)", new Vector2(150, 260));
            fireStatePattern.onClick.AddListener(() => _statePattern.PressTrigger());
            _statePatternLabel = DemoUiFactory.CreateLabel(canvas.transform, string.Empty, new Vector2(150, 220));
        }

        private void Update()
        {
            _baseline.Tick(Time.deltaTime);
            _statePattern.Tick(Time.deltaTime);

            _baselineLabel.text = $"[Baseline] {_baseline.State} (ammo: {_baseline.AmmoInMagazine})";
            _statePatternLabel.text = $"[StatePattern] ammo: {_statePattern.Context.AmmoInMagazine}";
        }
    }
}
