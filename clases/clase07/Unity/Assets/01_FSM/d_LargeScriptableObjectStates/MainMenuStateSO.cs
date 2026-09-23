using UnityEngine;

namespace Clase07.FSM.LargeScriptableObjectStates
{
    [CreateAssetMenu(fileName = "MainMenuState", menuName = "Clase07/FSM/Main Menu State")]
    public class MainMenuStateSO : GameFlowStateSO
    {
        public override string Name => "MainMenu";
    }
}
