using UnityEngine;

namespace Clase07.FSM.Large.ScriptableObjectStates
{
    // Cada estado es un asset: se puede crear, nombrar y (en un juego real)
    // configurar desde el Inspector sin tocar código.
    public abstract class GameFlowStateSO : ScriptableObject
    {
        public abstract string Name { get; }
        public virtual void Enter() { }
        public virtual void Tick(float deltaTime) { }
        public virtual void Exit() { }
    }
}
