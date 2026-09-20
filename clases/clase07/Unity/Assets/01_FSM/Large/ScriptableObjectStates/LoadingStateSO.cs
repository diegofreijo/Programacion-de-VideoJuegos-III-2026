using UnityEngine;

namespace Clase07.FSM.Large.ScriptableObjectStates
{
    [CreateAssetMenu(fileName = "LoadingState", menuName = "Clase07/FSM/Loading State")]
    public class LoadingStateSO : GameFlowStateSO
    {
        public override string Name => "Loading";
    }
}
