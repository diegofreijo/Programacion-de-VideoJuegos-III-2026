namespace Clase07.FSM.SmallStatePattern
{
    public interface IState
    {
        void OnEnter();
        void OnUpdate(float deltaTime);
        void OnExit();
    }
}
