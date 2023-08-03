using System;

namespace Terra.Studio
{
    public class GameStateHandler
    {
        private enum State { PreGame, Game, PostGame }
        private State currentGameState = State.PreGame;

        private event Action<object> OnGameStarted;

        public void SwitchToNextState()
        {
            var index = (int)currentGameState;
            var nextIndex = ++index;
            if (nextIndex == Enum.GetNames(typeof(State)).Length - 1)
            {
                //End of game
                return;
            }
            var nextState = (State)index;
            currentGameState = nextState;
            OnGameStarted?.Invoke(null);
        }

        public void SubscribeToGameStart(bool subscribe, Action<object> callback)
        {
            if (currentGameState == State.Game && subscribe)
            {
                OnGameStarted?.Invoke(null);
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
    }
}
