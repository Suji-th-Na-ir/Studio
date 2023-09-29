using System;
using UnityEngine;
using UnityEngine.UI;
using RTG;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class NavigationToolbar : View
    {

        private const string ORTHOGRAPHIC_LOC = "OrthographicButton";
        private const string PERSPECTIVE_LOC = "PerspectiveButton";
        private const string TOP_LOC = "Top";
        private const string BOTTOM_LOC = "Bottom";
        private const string LEFT_LOC = "Left";
        private const string RIGHT_LOC = "Right";
        private const string BACK_LOC = "Back";
        private const string FRONT_LOC = "Front";

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
           var topButtonTr = Helper.FindDeepChild(transform, TOP_LOC, true).GetComponent<Button>();
           var bottomButtonTr = Helper.FindDeepChild(transform, BOTTOM_LOC, true).GetComponent<Button>();
           var leftButtonTr = Helper.FindDeepChild(transform, LEFT_LOC, true).GetComponent<Button>();
           var rightButtonTr = Helper.FindDeepChild(transform, RIGHT_LOC, true).GetComponent<Button>();
           var frontButtonTr = Helper.FindDeepChild(transform, FRONT_LOC, true).GetComponent<Button>();
           var backButtonTr = Helper.FindDeepChild(transform, BACK_LOC, true).GetComponent<Button>();




            lastSelectedPerp = 0;
            SetPerspective();

            AddListenerEvent(orthoButtonTr, SetOrthographic);
            AddListenerEvent(perspButtonTr, SetPerspective);
            AddListenerEvent(topButtonTr, SetView,-Vector3.up);
            AddListenerEvent(bottomButtonTr, SetView,Vector3.up);
            AddListenerEvent(leftButtonTr, SetView,Vector3.right);
            AddListenerEvent(rightButtonTr, SetView,-Vector3.right);
            AddListenerEvent(frontButtonTr, SetView,Vector3.forward);
            AddListenerEvent(backButtonTr, SetView,-Vector3.forward);
        }

        private void SetView(Vector3 vector)
        {
            Quaternion targetRotation = Quaternion.LookRotation(vector, Vector3.up);
            EditorOp.Resolve<RTFocusCamera>().PerformRotationSwitch(targetRotation);
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

        private void AddListenerEvent(Button button, Action<Vector3> callback,Vector3 vector)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { callback?.Invoke(vector); });
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
