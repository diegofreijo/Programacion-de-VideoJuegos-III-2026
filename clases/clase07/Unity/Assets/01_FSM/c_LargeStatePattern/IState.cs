namespace Clase07.FSM.LargeStatePattern
{
    public interface IState
    {
        void OnEnter();
        void OnUpdate(float deltaTime);
        void OnExit();
    }
}
