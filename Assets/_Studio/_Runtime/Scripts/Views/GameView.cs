using UnityEngine;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class GameView : View
    {
        private const string PRE_GAME_UI_PATH = "Runtime/PreGameUI";
        private const string IN_GAME_UI_PATH = "Runtime/InGameUI";
        private const string POST_GAME_UI_PATH = "Runtime/PostGameUI";

        private View spawnedView;

        public override void Init()
        {
            var button = gameObject.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(SystemOp.Resolve<System>().SwitchState);
            RuntimeOp.Resolve<GameStateHandler>().SubscribeToStateChanged(true, Repaint);
            Draw();
        }

        public override void Draw()
        {
            var currentState = RuntimeOp.Resolve<GameStateHandler>().CurrentGameState;
            var uiToSpawn = GetUIPathFromState(currentState);
            var uiObj = Resources.Load<GameObject>(uiToSpawn);
            if (uiObj == null)
            {
                return;
            }
            var viewObj = Instantiate(uiObj, transform);
            viewObj.transform.SetAsFirstSibling();
            if (viewObj.TryGetComponent(out View view))
            {
                view.Init();
                spawnedView = view;
            }
        }

        private string GetUIPathFromState(GameStateHandler.State currentState)
        {
            return currentState switch
            {
                GameStateHandler.State.Game => IN_GAME_UI_PATH,
                GameStateHandler.State.PostGame => POST_GAME_UI_PATH,
                _ => PRE_GAME_UI_PATH,
            };
        }

        public override void Flush()
        {
            if (!spawnedView) return;
            spawnedView.Flush();
            Destroy(spawnedView.gameObject);
            spawnedView = null;
        }

        public override void Repaint()
        {
            Flush();
            Draw();
        }

        private void OnDestroy()
        {
            Flush();
            RuntimeOp.Resolve<GameStateHandler>()?.SubscribeToStateChanged(false, Repaint);
        }
    }
}
