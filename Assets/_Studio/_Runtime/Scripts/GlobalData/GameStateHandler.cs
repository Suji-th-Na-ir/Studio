using System;
using UnityEngine;

namespace Terra.Studio
{
    public class GameStateHandler
    {
        public enum State { PreGame, Game, PostGame }
        private State currentGameState = State.PreGame;
        public State CurrentGameState { get { return currentGameState; } }

        private event Action<object> OnGameStarted;
        private event Action<object> OnGameEnded;
        private event Action OnStateChanged;

        public void SwitchToNextState()
        {
            if (currentGameState == State.PostGame)
            {
                return;
            }
            var index = (int)currentGameState;
            var nextIndex = ++index;
            if (nextIndex == Enum.GetNames(typeof(State)).Length - 1)
            {
                Debug.Log("Game ended");
                currentGameState = (State)index;
                OnGameEnded?.Invoke(null);
            }
            else
            {
                var nextState = (State)index;
                currentGameState = nextState;
                OnGameStarted?.Invoke(null);
            }
            OnStateChanged?.Invoke();
        }

        public void SubscribeToGameStart(bool subscribe, Action<object> callback)
        {
            if (currentGameState == State.Game && subscribe)
            {
                WaitForAFrameAndProvideCallback(callback);
                return;
            }
            if (callback == null)
            {
                return;
            }
            if (subscribe)
            {
                OnGameStarted += callback;
            }
            else
            {
                OnGameStarted -= callback;
            }
        }

        public void SubscribeToGameEnd(bool subscribe, Action<object> callback)
        {
            if (currentGameState == State.PostGame && subscribe)
            {
                WaitForAFrameAndProvideCallback(callback);
                return;
            }
            if (callback == null)
            {
                return;
            }
            if (subscribe)
            {
                OnGameEnded += callback;
            }
            else
            {
                OnGameEnded -= callback;
            }
        }

        public void SubscribeToStateChanged(bool subscribe, Action callback)
        {
            if (callback == null)
            {
                return;
            }
            if (subscribe)
            {
                OnStateChanged += callback;
            }
            else
            {
                OnStateChanged -= callback;
            }
        }

        private void WaitForAFrameAndProvideCallback(Action<object> callback)
        {
            CoroutineService.RunCoroutine(() =>
            {
                callback?.Invoke(null);
            },
            CoroutineService.DelayType.WaitForFrame);
        }
    }
}
