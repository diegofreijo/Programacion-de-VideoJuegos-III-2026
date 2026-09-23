using UnityEngine;
using TMPro;

namespace Clase07.DI.Singleton
{
    public class DiSingletonDemoBootstrapper : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private void Awake()
        {
            if (ScoreServiceSingleton.Instance == null)
            {
                new GameObject("ScoreServiceSingleton").AddComponent<ScoreServiceSingleton>().Initialize();
            }
            if (AudioServiceSingleton.Instance == null)
            {
                new GameObject("AudioServiceSingleton").AddComponent<AudioServiceSingleton>().Initialize();
            }
            _scoreLabel.text = "Score: 0";
        }

        public void OnCoinClicked()
        {
            ScoreServiceSingleton.Instance.Service.AddScore(10);
            AudioServiceSingleton.Instance.Service.PlayCoinSound();
            _scoreLabel.text = $"Score: {ScoreServiceSingleton.Instance.Service.CurrentScore}";
        }
    }
}
