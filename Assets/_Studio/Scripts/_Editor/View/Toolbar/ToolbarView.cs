using System;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class ToolbarView : View
    {
        private const string PLAY_BUTTON_LOC = "PlayButton";

        private void Awake()
        {
            Interop<EditorInterop>.Current.Register(this);
        }

        public override void Init()
        {
            var playButtonTr = Helper.FindDeepChild(transform, PLAY_BUTTON_LOC, true);
            var playButton = playButtonTr.GetComponent<Button>();
            AddListenerEvent(playButton, Interop<EditorInterop>.Current.Resolve<EditorSystem>().RequestSwitchState);
        }

        public override void Draw()
        {
            //Nothing to draw
        }

        public override void Flush()
        {
            //Nothing to flush here
        }

        public override void Repaint()
        {
            //Nothing to re-paint
        }

        private void AddListenerEvent(Button button, Action callback)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { callback?.Invoke(); });
        }

        private void OnDestroy()
        {
            Interop<EditorInterop>.Current.Unregister(this);
        }
    }
}
