using System;
using UnityEngine;

namespace Terra.Studio
{
    public class SceneView : View
    {
        public Action OnAnimationDone;
        private const string TOGGLE_OUT_STATE_KEY = "Sine Out";
        private const string TOGGLE_IN_STATE_KEY = "Sine In";
        private Animator previewToggleAnim;

        private void Awake()
        {
            EditorOp.Register(this);
        }

        public override void Init()
        {
            previewToggleAnim = GetComponent<Animator>();
        }

        public override void Draw()
        {

        }

        public override void Flush()
        {
            Destroy(this);
        }

        public override void Repaint()
        {

        }

        public void TogglePreviewAnim(bool sineOut)
        {
            var clipName = sineOut ? TOGGLE_OUT_STATE_KEY : TOGGLE_IN_STATE_KEY;
            previewToggleAnim.Play(clipName);
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }
    }
}
