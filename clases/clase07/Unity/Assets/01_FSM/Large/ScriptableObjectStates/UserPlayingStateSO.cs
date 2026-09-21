using UnityEngine;

namespace Clase07.FSM.Large.ScriptableObjectStates
{
    [CreateAssetMenu(fileName = "UserPlayingState", menuName = "Clase07/FSM/User Playing State")]
    public class UserPlayingStateSO : GameFlowStateSO
    {
        public override string Name => "UserPlaying";
    }
}
