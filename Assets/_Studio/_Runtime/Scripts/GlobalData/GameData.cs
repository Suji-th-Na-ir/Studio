using UnityEngine;

namespace Terra.Studio
{
    public class GameData
    {
        public Vector3 RespawnPoint;
        public Transform PlayerRef;
        public GameEndState EndState;

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

        public void SetPlayerPosition(Vector3 position)
        {
            if (PlayerRef)
            {
                PlayerRef.position = position;
            }
        }
    }
}
