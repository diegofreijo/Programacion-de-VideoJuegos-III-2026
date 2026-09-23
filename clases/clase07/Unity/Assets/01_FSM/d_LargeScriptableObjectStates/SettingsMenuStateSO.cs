using UnityEngine;

namespace Clase07.FSM.LargeScriptableObjectStates
{
    [CreateAssetMenu(fileName = "SettingsMenuState", menuName = "Clase07/FSM/Settings Menu State")]
    public class SettingsMenuStateSO : GameFlowStateSO
    {
        public override string Name => "SettingsMenu";
    }
}
