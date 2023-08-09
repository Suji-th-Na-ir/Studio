using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class CoreGameManager : IDisposable
    {
        private const string PLAYER_RESOURCE_PATH = "Runtime/Player";
        private const string GAME_VIEW_UI_PATH = "Runtime/GameViewCanvas";

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
            var playerObj = Resources.Load<GameObject>(PLAYER_RESOURCE_PATH);
            var reference = Object.Instantiate(playerObj);
            RuntimeOp.Resolve<GameData>().PlayerRef = reference.transform;
        }

        private void SpawnGameUI()
        {
            var gameUI = Resources.Load<GameObject>(GAME_VIEW_UI_PATH);
            var reference = Object.Instantiate(gameUI);
            if (reference.TryGetComponent(out GameView view)) view.Init();
        }

        public void Dispose()
        {
            RuntimeOp.Unregister<GameData>();
            RuntimeOp.Unregister<GameStateHandler>();
        }
    }
}
