using System;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class ToolbarView : View
    {
        private const string PLAY_BUTTON_LOC = "PlayButton";
        private const string SAVE_BUTTON_LOC = "SaveButton";
        private const string LOAD_BUTTON_LOC = "LoadButton";

        private void Awake()
        {
            Interop<EditorInterop>.Current.Register(this);
        }

        public override void Init()
        {
            var playButtonTr = Helper.FindDeepChild(transform, PLAY_BUTTON_LOC, true);
            var saveButtonTr = Helper.FindDeepChild(transform, SAVE_BUTTON_LOC, true);
            var loadButtonTr = Helper.FindDeepChild(transform, LOAD_BUTTON_LOC, true);

            var playButton = playButtonTr.GetComponent<Button>();
            AddListenerEvent(playButton, () =>
            {
                var scene = Interop<EditorInterop>.Current.Resolve<SceneExporter>().ExportJson();
                Interop<SystemInterop>.Current.Resolve<CrossSceneDataHolder>().Set(scene);
                Interop<EditorInterop>.Current.Resolve<EditorSystem>().RequestSwitchState();
            });

            var saveButton = saveButtonTr.GetComponent<Button>();
            AddListenerEvent(saveButton, Interop<EditorInterop>.Current.Resolve<EditorSystem>().RequestSaveScene);

            var loadButton = loadButtonTr.GetComponent<Button>();
            AddListenerEvent(loadButton, Interop<EditorInterop>.Current.Resolve<EditorSystem>().RequestLoadScene);
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
