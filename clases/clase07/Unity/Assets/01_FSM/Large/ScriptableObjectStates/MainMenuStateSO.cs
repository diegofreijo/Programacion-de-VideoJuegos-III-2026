using UnityEngine;

namespace Clase07.FSM.Large.ScriptableObjectStates
{
    [CreateAssetMenu(fileName = "MainMenuState", menuName = "Clase07/FSM/Main Menu State")]
    public class MainMenuStateSO : GameFlowStateSO
    {
        public override string Name => "MainMenu";
    }
}
