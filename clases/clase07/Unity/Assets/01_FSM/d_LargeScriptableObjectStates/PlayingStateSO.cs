using UnityEngine;

namespace Clase07.FSM.LargeScriptableObjectStates
{
    [CreateAssetMenu(fileName = "PlayingState", menuName = "Clase07/FSM/Playing State")]
    public class PlayingStateSO : GameFlowStateSO
    {
        public override string Name => "Playing";

        // A diferencia de c_LargeStatePattern, acá "a qué estado puedo ir" no es
        // código — son tres referencias serializadas, arrastrables desde el
        // Inspector sin tocar PlayingStateSO.cs.
        [SerializeField] private UserPlayingStateSO _userPlaying;
        [SerializeField] private PauseMenuStateSO _pauseMenu;
        [SerializeField] private SettingsMenuStateSO _settingsMenu;

        public void Configure(UserPlayingStateSO userPlaying, PauseMenuStateSO pauseMenu, SettingsMenuStateSO settingsMenu)
        {
            _userPlaying = userPlaying;
            _pauseMenu = pauseMenu;
            _settingsMenu = settingsMenu;
        }

        private GameFlowStateSO _currentSubstate;
        public string CurrentSubstateName => _currentSubstate.Name;

        public override void Enter() => ChangeSubstate(_userPlaying);
        public override void Tick(float deltaTime) => _currentSubstate.Tick(deltaTime);

        public void Pause()
        {
            if (_currentSubstate == _userPlaying) ChangeSubstate(_pauseMenu);
        }

        public void Resume()
        {
            if (_currentSubstate == _pauseMenu) ChangeSubstate(_userPlaying);
        }

        public void OpenSettings()
        {
            if (_currentSubstate == _pauseMenu) ChangeSubstate(_settingsMenu);
        }

        public void CloseSettings()
        {
            if (_currentSubstate == _settingsMenu) ChangeSubstate(_pauseMenu);
        }

        private void ChangeSubstate(GameFlowStateSO next)
        {
            if (_currentSubstate == next) return;
            _currentSubstate?.Exit();
            _currentSubstate = next;
            _currentSubstate.Enter();
        }
    }
}
