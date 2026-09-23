using UnityEngine;

namespace Clase07.FSM.LargeScriptableObjectStates
{
    [CreateAssetMenu(fileName = "LoadingState", menuName = "Clase07/FSM/Loading State")]
    public class LoadingStateSO : GameFlowStateSO
    {
        public override string Name => "Loading";
    }
}
