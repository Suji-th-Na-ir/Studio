using System;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class CoreGameManager : IDisposable
    {
        private Dictionary<Type, List<Action>> callbackOnModuleActivation = new();
        private Action onDisposeInvoked;

        public CoreGameManager()
        {
            Initialize();
        }

        private void Initialize()
        {
            RuntimeOp.Register(new GameData());
            RuntimeOp.Register(new GameStateHandler());
            SpawnPlayer();
            SpawnGameUI();
        }

        private void SpawnPlayer()
        {
            var playerObj = (GameObject)RuntimeOp.Load(ResourceTag.Player);
            var spawnPoint = RuntimeOp.Resolve<GameData>().SpawnPoint;

            if (spawnPoint != null)
            {
                var reference = Object.Instantiate(playerObj, spawnPoint.transform.position+new Vector3(0,2,0),spawnPoint.transform.rotation);
                RuntimeOp.Resolve<GameData>().PlayerRef = reference.transform;
            }
            else
            {
                var reference = Object.Instantiate(playerObj);
                RuntimeOp.Resolve<GameData>().PlayerRef = reference.transform;
            }
        }

        private void SpawnGameUI()
        {
            var gameUI = RuntimeOp.Load<GameObject>("GameViewCanvas");
            var reference = Object.Instantiate(gameUI);
            if (reference.TryGetComponent(out GameView view)) view.Init();
        }

        public void Dispose()
        {
            onDisposeInvoked?.Invoke();
            RuntimeOp.Unregister<GameData>();
            RuntimeOp.Unregister<GameStateHandler>();
        }

        public void EnableModule<T>()
        {
            if (RuntimeOp.Resolve<T>() != null) return;
            var type = typeof(T);
            switch (type.Name)
            {
                case nameof(ScoreHandler):
                    RuntimeOp.Register(new ScoreHandler());
                    InvokeModuleActivationCall(type);
                    onDisposeInvoked += () => { RuntimeOp.Unregister<ScoreHandler>(); };
                    break;
                case nameof(InGameTimeHandler):
                    RuntimeOp.Register(new InGameTimeHandler());
                    InvokeModuleActivationCall(type);
                    onDisposeInvoked += () => { RuntimeOp.Unregister<InGameTimeHandler>(); };
                    break;
            }
        }

        public void GetCallbackOnModuleActivation<T>(Action callback)
        {
            var type = typeof(T);
            if (RuntimeOp.Resolve<T>() != null)
            {
                callback?.Invoke();
                return;
            }
            if (!callbackOnModuleActivation.ContainsKey(type))
            {
                callbackOnModuleActivation.Add(type, new List<Action>());
            }
            callbackOnModuleActivation[type].Add(callback);
        }

        private void InvokeModuleActivationCall(Type type)
        {
            if (!callbackOnModuleActivation.ContainsKey(type))
            {
                return;
            }
            foreach (var callback in callbackOnModuleActivation[type])
            {
                callback?.Invoke();
            }
            callbackOnModuleActivation.Remove(type);
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

    public class InGameTimeHandler
    {
        private float currentTime;
        public float CurrentTime { get { return currentTime; } }
        public event Action<float> OnTimeModified;

        public void UpdateTime(float newTime)
        {
            currentTime = newTime;
            OnTimeModified?.Invoke(newTime);
        }
    }
}
