using UnityEngine;

namespace Clase07.FSM.LargeScriptableObjectStates
{
    [CreateAssetMenu(fileName = "PauseMenuState", menuName = "Clase07/FSM/Pause Menu State")]
    public class PauseMenuStateSO : GameFlowStateSO
    {
        public override string Name => "PauseMenu";
    }
}
