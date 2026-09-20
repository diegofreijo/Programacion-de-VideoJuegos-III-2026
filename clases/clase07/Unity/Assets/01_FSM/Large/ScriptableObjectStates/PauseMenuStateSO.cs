using UnityEngine;

namespace Clase07.FSM.Large.ScriptableObjectStates
{
    [CreateAssetMenu(fileName = "PauseMenuState", menuName = "Clase07/FSM/Pause Menu State")]
    public class PauseMenuStateSO : GameFlowStateSO
    {
        public override string Name => "PauseMenu";
    }
}
