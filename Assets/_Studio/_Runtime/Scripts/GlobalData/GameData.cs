using System;
using UnityEngine;

namespace Terra.Studio
{
    public class GameData : IDisposable
    {
        public Vector3 RespawnPoint;
        public GameEndState EndState;

        public GameData()
        {
            RuntimeOp.Register(new PlayerData());
        }

        public void SetEndState(string state)
        {
            if (state.Equals("Game Win"))
            {
                EndState = GameEndState.Win;
            }
            else
            {
                EndState = GameEndState.Lose;
            }
        }

        public void Dispose()
        {
            RuntimeOp.Unregister<PlayerData>();
        }
    }
}