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
            IntializeDataManagers();
            SpawnGameUI();
            RuntimeOp.Resolve<GameStateHandler>().SubscribeToGameStart(true, (data) => { RuntimeOp.Resolve<PlayerData>().SpawnPlayer(); });
        }

        public void IntializeDataManagers()
        {
            RuntimeOp.Register(new GameData());
            RuntimeOp.Register(new GameStateHandler());
        }

        private void SpawnGameUI()
        {
            var canSpawn = SystemOp.Resolve<System>().CanInitiateSubsystemProcess?.Invoke() ?? true;
            if (!canSpawn) return;
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
        public int targetScore;
        public int currentScore;
        public int CurrentScore { get { return currentScore; } }
        public event Action<int> OnScoreModified;
        public event Action OnScoreReset;
        public event Action OnTargetScoreReached;

        public void AddScore(int addBy)
        {
            currentScore += addBy;
            OnScoreModified?.Invoke(currentScore);
            if (currentScore >= targetScore)
            {
                OnTargetScoreReached?.Invoke();
            }
        }

        public void ResetScore()
        {
            currentScore = 0;
            OnScoreModified?.Invoke(currentScore);
            OnScoreReset?.Invoke();
        }
    }

    public class InGameTimeHandler
    {
        private float startTime;
        private float currentTime;
        public float CurrentTime { get { return currentTime; } }
        public event Action<float> OnTimeModified;

        public void SetTime(float startTime)
        {
            this.startTime = startTime;
            currentTime = startTime;
        }

        public void UpdateTime(float newTime)
        {
            currentTime = newTime;
            OnTimeModified?.Invoke(newTime);
        }

        public void AddTime(float newTime)
        {
            currentTime += newTime;
            if (currentTime < 0f)
            {
                currentTime = 0f;
            }
        }

        public void ResetTime()
        {
            currentTime = startTime;
        }
    }
}
