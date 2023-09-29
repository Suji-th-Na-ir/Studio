using System;
using PlayShifu.Terra;
using UnityEngine.UI;
using RTG;
namespace Terra.Studio
{
    public class NavigationToolbar : View
    {

        private const string ORTHOGRAPHIC_LOC = "OrthographicButton";
        private const string PERSPECTIVE_LOC = "PerspectiveButton";

        private int lastSelectedPerp = 1;
        private Button orthoButtonTr;
        private Button perspButtonTr;

        private void Awake()
        {
            EditorOp.Register(this);
        }

        public override void Init()
        {
            orthoButtonTr = Helper.FindDeepChild(transform, ORTHOGRAPHIC_LOC, true).GetComponent<Button>();
            perspButtonTr = Helper.FindDeepChild(transform, PERSPECTIVE_LOC, true).GetComponent<Button>();
            lastSelectedPerp = 0;
            SetPerspective();

            AddListenerEvent(orthoButtonTr, SetOrthographic);
            AddListenerEvent(perspButtonTr, SetPerspective);
        }

        private void SetPerspective()
        {
            if(lastSelectedPerp!=0)
            EditorOp.Resolve<RTFocusCamera>().PerformProjectionSwitch();
            orthoButtonTr.GetComponent<Image>().color = Helper.GetColorFromHex("#656565");
            perspButtonTr.GetComponent<Image>().color = Helper.GetColorFromHex("#161720");
            lastSelectedPerp = 0;
        }

        private void SetOrthographic()
        {
            if (lastSelectedPerp != 1)
                EditorOp.Resolve<RTFocusCamera>().PerformProjectionSwitch();
            perspButtonTr.GetComponent<Image>().color = Helper.GetColorFromHex("#656565");
            orthoButtonTr.GetComponent<Image>().color = Helper.GetColorFromHex("#161720");
            lastSelectedPerp = 1;
        }

        public override void Draw()
        {
          //No Draw
        }

        public override void Flush()
        {
          //No Flush
        }

        private void AddListenerEvent(Button button, Action callback)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { callback?.Invoke(); });
        }

        public override void Repaint()
        {
          //No repaint
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }

    }
}
