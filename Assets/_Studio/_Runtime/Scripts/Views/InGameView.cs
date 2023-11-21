using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Terra.Studio
{
    public class InGameView : View
    {
        [SerializeField] private GameObject scoreGo;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private GameObject timerGo;
        [SerializeField] private TextMeshProUGUI timerText;

        private int entity = int.MinValue;
        private Dictionary<string, GameObject> dynamicSpawnedGO = new();

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
        }

        public override void Draw()
        {
            var score = RuntimeOp.Resolve<ScoreHandler>().CurrentScore;
            SetScore(score);
        }

        public override void Flush()
        {

        }

        public override void Repaint()
        {

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
