using System;

namespace Clase07.FSM.SmallStatePattern
{
    // Copia local del motor genérico — esta carpeta es autocontenida a propósito,
    // ver c_LargeStatePattern/StateMachine.cs para la otra copia independiente.
    public class StateMachine<TState> where TState : class, IState
    {
        public TState CurrentState { get; private set; }
        public event Action<TState, TState> StateChanged;

        public StateMachine(TState initialState)
        {
            CurrentState = initialState ?? throw new ArgumentNullException(nameof(initialState));
            CurrentState.OnEnter();
        }

        public void ChangeState(TState next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            if (ReferenceEquals(next, CurrentState)) return;

            var previous = CurrentState;
            previous.OnExit();
            CurrentState = next;
            CurrentState.OnEnter();
            StateChanged?.Invoke(previous, CurrentState);
        }

        public void Tick(float deltaTime) => CurrentState.OnUpdate(deltaTime);
    }
}
