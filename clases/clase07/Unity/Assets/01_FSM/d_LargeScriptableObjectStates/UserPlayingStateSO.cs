using UnityEngine;

namespace Clase07.FSM.LargeScriptableObjectStates
{
    [CreateAssetMenu(fileName = "UserPlayingState", menuName = "Clase07/FSM/User Playing State")]
    public class UserPlayingStateSO : GameFlowStateSO
    {
        public override string Name => "UserPlaying";
    }
}
