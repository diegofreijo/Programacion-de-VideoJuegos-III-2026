using UnityEngine;

namespace Clase07.FSM.Large.ScriptableObjectStates
{
    [CreateAssetMenu(fileName = "SettingsMenuState", menuName = "Clase07/FSM/Settings Menu State")]
    public class SettingsMenuStateSO : GameFlowStateSO
    {
        public override string Name => "SettingsMenu";
    }
}
