using System;
using UnityEngine;
using UnityEngine.UI;
using RTG;
using PlayShifu.Terra;
using System.Collections.Generic;

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
       
        private Button orthoButtonTr;
        private Button perspButtonTr;
        List<Button> viewButtons = new List<Button>();
        List<Text> viewButtonsTexts = new List<Text>();
        private int lastSelectedPerp = 1;

        private void Awake()
        {
            EditorOp.Register(this);
        }

        public override void Init()
        {
            orthoButtonTr = Helper.FindDeepChild(transform, ORTHOGRAPHIC_LOC, true).GetComponent<Button>();
            perspButtonTr = Helper.FindDeepChild(transform, PERSPECTIVE_LOC, true).GetComponent<Button>();

            viewButtons.Add( Helper.FindDeepChild(transform, TOP_LOC, true).GetComponent<Button>());
            viewButtons.Add(Helper.FindDeepChild(transform, BOTTOM_LOC, true).GetComponent<Button>());
            viewButtons.Add(Helper.FindDeepChild(transform, LEFT_LOC, true).GetComponent<Button>());
            viewButtons.Add(Helper.FindDeepChild(transform, RIGHT_LOC, true).GetComponent<Button>());
            viewButtons.Add(Helper.FindDeepChild(transform, FRONT_LOC, true).GetComponent<Button>());
            viewButtons.Add(Helper.FindDeepChild(transform, BACK_LOC, true).GetComponent<Button>());

            for (int i = 0; i < viewButtons.Count; i++)
            {
                viewButtonsTexts.Add(viewButtons[i].GetComponentInChildren<Text>());
            }

            lastSelectedPerp = 0;
            SetPerspective();

            AddListenerEvent(orthoButtonTr, SetOrthographic);
            AddListenerEvent(perspButtonTr, SetPerspective);
            AddListenerEvent(viewButtons[0], SetView,-Vector3.up);
            AddListenerEvent(viewButtons[1], SetView,Vector3.up);
            AddListenerEvent(viewButtons[2], SetView,Vector3.right);
            AddListenerEvent(viewButtons[3], SetView,-Vector3.right);
            AddListenerEvent(viewButtons[4], SetView,Vector3.forward);
            AddListenerEvent(viewButtons[5], SetView,-Vector3.forward);

            EditorOp.Resolve<RTFocusCamera>().OnCameraMoved += ResetSelectedView;
        }

        private void ResetSelectedView()
        {
            for (int i = 0; i < viewButtonsTexts.Count; i++)
                viewButtonsTexts[i].color = Helper.GetColorFromHex("#F3F3F3");
        }

        private void SetView(Vector3 vector,Button button)
        {
            for (int i = 0; i < viewButtons.Count; i++)
            {
                if (viewButtons[i]==button)
                {
                    viewButtonsTexts[i].color = Helper.GetColorFromHex("#4A2BE9");
                }
                else
                {
                    viewButtonsTexts[i].color = Helper.GetColorFromHex("#F3F3F3");
                }
            }
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

        private void AddListenerEvent(Button button, Action<Vector3,Button> callback,Vector3 vector)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { callback?.Invoke(vector,button); });
        }

        public override void Repaint()
        {
            EditorOp.Resolve<RTFocusCamera>().OnCameraMoved -= ResetSelectedView;
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }

    }
}
