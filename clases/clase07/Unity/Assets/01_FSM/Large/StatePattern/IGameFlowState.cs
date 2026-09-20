using Clase07.FSM.Core;

namespace Clase07.FSM.Large.StatePattern
{
    public interface IGameFlowState : IState
    {
        string Name { get; }
    }
}
