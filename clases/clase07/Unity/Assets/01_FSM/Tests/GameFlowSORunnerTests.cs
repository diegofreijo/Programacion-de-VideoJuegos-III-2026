using NUnit.Framework;
using Clase07.FSM.Large.ScriptableObjectStates;

namespace Clase07.FSM.Tests
{
    public class GameFlowSORunnerTests
    {
        private GameFlowSORunner BuildRunner()
        {
            var userPlaying = UnityEngine.ScriptableObject.CreateInstance<UserPlayingStateSO>();
            var pauseMenu = UnityEngine.ScriptableObject.CreateInstance<PauseMenuStateSO>();
            var settingsMenu = UnityEngine.ScriptableObject.CreateInstance<SettingsMenuStateSO>();
            var playing = UnityEngine.ScriptableObject.CreateInstance<PlayingStateSO>();
            playing.Configure(userPlaying, pauseMenu, settingsMenu);
            var loading = UnityEngine.ScriptableObject.CreateInstance<LoadingStateSO>();
            var mainMenu = UnityEngine.ScriptableObject.CreateInstance<MainMenuStateSO>();

            var runner = new GameFlowSORunner();
            runner.Initialize(loading, mainMenu, playing);
            return runner;
        }

        [Test]
        public void StartsInLoading()
        {
            Assert.AreEqual("Loading", BuildRunner().CurrentState.Name);
        }

        [Test]
        public void Play_EntersPlayingWithUserPlayingSubstate()
        {
            var runner = BuildRunner();
            runner.FinishLoading();
            runner.Play();

            Assert.AreEqual("Playing", runner.CurrentState.Name);
            Assert.AreEqual("UserPlaying", runner.CurrentSubstateName);
        }

        [Test]
        public void PauseThenOpenSettingsThenClose_ReturnsToPauseMenu()
        {
            var runner = BuildRunner();
            runner.FinishLoading();
            runner.Play();
            runner.Pause();

            runner.OpenSettings();
            Assert.AreEqual("SettingsMenu", runner.CurrentSubstateName);

            runner.CloseSettings();
            Assert.AreEqual("PauseMenu", runner.CurrentSubstateName);
        }

        [Test]
        public void Resume_ReturnsToUserPlaying()
        {
            var runner = BuildRunner();
            runner.FinishLoading();
            runner.Play();
            runner.Pause();

            runner.Resume();

            Assert.AreEqual("UserPlaying", runner.CurrentSubstateName);
        }
    }
}
