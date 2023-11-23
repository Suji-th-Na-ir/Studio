using TMPro;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class InGameView : View
    {
        [SerializeField] private GameObject scoreGo;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private GameObject timerGo;
        [SerializeField] private TextMeshProUGUI timerText;

        private Dictionary<string, GameObject> dynamicSpawnedGO;

        public override void Init()
        {
            RuntimeOp.Resolve<CoreGameManager>().GetCallbackOnModuleActivation<ScoreHandler>(() =>
            {
                scoreGo.SetActive(true);
                RuntimeOp.Resolve<ScoreHandler>().OnScoreModified += SetScore;
            });
            RuntimeOp.Resolve<CoreGameManager>().GetCallbackOnModuleActivation<InGameTimeHandler>(() =>
            {
                timerGo.SetActive(true);
                RuntimeOp.Resolve<InGameTimeHandler>().OnTimeModified += SetTime;
            });
            dynamicSpawnedGO = new();
        }

        public override void Draw()
        {
            var score = RuntimeOp.Resolve<ScoreHandler>().CurrentScore;
            SetScore(score);
        }

        private void SetScore(int currentScore)
        {
            scoreText.text = currentScore.ToString();
        }

        private void SetTime(float currentTime)
        {
            var minutes = Mathf.FloorToInt(currentTime / 60);
            var seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public override GameObject AttachDynamicUI(string component, GameObject go)
        {
            if (dynamicSpawnedGO.TryGetValue(component, out var value))
            {
                return value;
            }

            var newUI = Instantiate(go, transform);
            dynamicSpawnedGO.Add(component, newUI);
            return newUI;
        }

        public override void RemoveDynamicUI(string component)
        {
            if (dynamicSpawnedGO.TryGetValue(component, out var value))
            {
                Destroy(value);
                dynamicSpawnedGO.Remove(component);
            }
        }
    }
}
