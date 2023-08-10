using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class CoreGameManager : IDisposable
    {
        public CoreGameManager()
        {
            Initialize();
        }

        private void Initialize()
        {
            RuntimeOp.Register(new GameData());
            RuntimeOp.Register(new GameStateHandler());
            RuntimeOp.Register(new ScoreHandler());
            SpawnPlayer();
            SpawnGameUI();
        }

        private void SpawnPlayer()
        {
            var playerObj = (GameObject)RuntimeOp.Load(ResourceTag.Player);
            var reference = Object.Instantiate(playerObj);
            RuntimeOp.Resolve<GameData>().PlayerRef = reference.transform;
        }

        private void SpawnGameUI()
        {
            var gameUI = RuntimeOp.Load<GameObject>("GameViewCanvas");
            var reference = Object.Instantiate(gameUI);
            if (reference.TryGetComponent(out GameView view)) view.Init();
        }

        public void Dispose()
        {
            RuntimeOp.Unregister<GameData>();
            RuntimeOp.Unregister<GameStateHandler>();
            RuntimeOp.Unregister<ScoreHandler>();
        }
    }

    public class ScoreHandler
    {
        public int currentScore;
        public int CurrentScore { get { return currentScore; } }
        public event Action<int> OnScoreModified;

        public void AddScore(int addBy)
        {
            currentScore += addBy;
            OnScoreModified?.Invoke(currentScore);
        }

        public void RemoveScore(int removeBy)
        {
            currentScore -= removeBy;
            OnScoreModified?.Invoke(currentScore);
        }
    }
}
