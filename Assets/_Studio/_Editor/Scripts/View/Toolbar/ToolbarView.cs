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
            EditorOp.Register(this);
        }

        public override void Init()
        {
            var playButtonTr = Helper.FindDeepChild(transform, PLAY_BUTTON_LOC, true);
            var saveButtonTr = Helper.FindDeepChild(transform, SAVE_BUTTON_LOC, true);
            var loadButtonTr = Helper.FindDeepChild(transform, LOAD_BUTTON_LOC, true);

            var playButton = playButtonTr.GetComponent<Button>();
            AddListenerEvent(playButton, () =>
            {
                var scene = EditorOp.Resolve<SceneExporter>().ExportJson();
                SystemOp.Resolve<CrossSceneDataHolder>().Set(scene);
                EditorOp.Resolve<EditorSystem>().RequestSwitchState();
            });

            var saveButton = saveButtonTr.GetComponent<Button>();
            AddListenerEvent(saveButton, EditorOp.Resolve<EditorSystem>().RequestSaveScene);

            var loadButton = loadButtonTr.GetComponent<Button>();
            AddListenerEvent(loadButton, EditorOp.Resolve<EditorSystem>().RequestLoadScene);
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
            EditorOp.Unregister(this);
        }


    }
}
