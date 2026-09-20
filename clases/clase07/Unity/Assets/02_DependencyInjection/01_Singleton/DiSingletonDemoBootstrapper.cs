using UnityEngine;
using UnityEngine.UI;
using Clase07.Shared.UI;

namespace Clase07.DI.Singleton
{
    public class DiSingletonDemoBootstrapper : MonoBehaviour
    {
        private Text _scoreLabel;

        private void Awake() => Build();

        public void Build()
        {
            if (ScoreServiceSingleton.Instance == null)
            {
                new GameObject("ScoreServiceSingleton").AddComponent<ScoreServiceSingleton>().Initialize();
            }
            if (AudioServiceSingleton.Instance == null)
            {
                new GameObject("AudioServiceSingleton").AddComponent<AudioServiceSingleton>().Initialize();
            }

            var canvas = DemoUiFactory.CreateCanvas();
            var button = DemoUiFactory.CreateButton(canvas.transform, "Coin (Singleton)", new Vector2(0, 100));
            _scoreLabel = DemoUiFactory.CreateLabel(canvas.transform, "Score: 0", new Vector2(0, 60));

            button.onClick.AddListener(() =>
            {
                ScoreServiceSingleton.Instance.Service.AddScore(10);
                AudioServiceSingleton.Instance.Service.PlayCoinSound();
                _scoreLabel.text = $"Score: {ScoreServiceSingleton.Instance.Service.CurrentScore}";
            });
        }
    }
}
