using NUnit.Framework;
using Clase07.FSM.LargeStatePattern;

namespace Clase07.FSM.LargeStatePattern.Tests
{
    public class GameFlowControllerTests
    {
        [Test]
        public void StartsInLoading()
        {
            var flow = new GameFlowController();
            Assert.AreEqual("Loading", flow.CurrentState.Name);
        }

        [Test]
        public void FinishLoading_MovesToMainMenu()
        {
            var flow = new GameFlowController();
            flow.FinishLoading();
            Assert.AreEqual("MainMenu", flow.CurrentState.Name);
        }

        [Test]
        public void Play_EntersPlayingWithUserPlayingSubstate()
        {
            var flow = new GameFlowController();
            flow.FinishLoading();
            flow.Play();

            Assert.AreEqual("Playing", flow.CurrentState.Name);
            Assert.AreEqual("UserPlaying", flow.CurrentSubstateName);
        }

        [Test]
        public void Pause_MovesSubstateToPauseMenu_WithoutLeavingPlaying()
        {
            var flow = new GameFlowController();
            flow.FinishLoading();
            flow.Play();

            flow.Pause();

            Assert.AreEqual("Playing", flow.CurrentState.Name);
            Assert.AreEqual("PauseMenu", flow.CurrentSubstateName);
        }

        [Test]
        public void OpenSettingsThenClose_ReturnsToPauseMenu()
        {
            var flow = new GameFlowController();
            flow.FinishLoading();
            flow.Play();
            flow.Pause();

            flow.OpenSettings();
            Assert.AreEqual("SettingsMenu", flow.CurrentSubstateName);

            flow.CloseSettings();
            Assert.AreEqual("PauseMenu", flow.CurrentSubstateName);
        }

        [Test]
        public void Resume_ReturnsToUserPlaying()
        {
            var flow = new GameFlowController();
            flow.FinishLoading();
            flow.Play();
            flow.Pause();

            flow.Resume();

            Assert.AreEqual("UserPlaying", flow.CurrentSubstateName);
        }

        [Test]
        public void Pause_WhileAlreadyPaused_IsNoOp()
        {
            var flow = new GameFlowController();
            flow.FinishLoading();
            flow.Play();
            flow.Pause();

            flow.Pause();

            Assert.AreEqual("PauseMenu", flow.CurrentSubstateName);
        }

        [Test]
        public void CurrentSubstateName_IsNoneOutsidePlaying()
        {
            var flow = new GameFlowController();
            Assert.AreEqual("None", flow.CurrentSubstateName);

            flow.FinishLoading();
            Assert.AreEqual("None", flow.CurrentSubstateName);
        }
    }
}
