using System.Runtime.InteropServices;
using UnityEngine;
using RuntimeInspectorNamespace;
using UnityEngine.EventSystems;
using PlayShifu.Terra;

namespace CMF
{
	//This character movement input class is an example of how to get input from a keyboard to control the character;
    public class CharacterKeyboardInput : CharacterInput
    {
		public string horizontalInputAxis = "Horizontal";
		public string verticalInputAxis = "Vertical";
		public KeyCode jumpKey = KeyCode.Space;
		[SerializeField] Joystick joystick;
		[SerializeField] PointerEventListener JumpButton;
		[SerializeField] GameObject mobileInputCanvas;
		//If this is enabled, Unity's internal input smoothing is bypassed;
		public bool useRawInput = true;
		bool isJumpPressed;



		private void Awake()
		{
			if (!Helper.IsMobileWebGlPlatform())
			{
				mobileInputCanvas.SetActive(false);
			}
			else
			{
				JumpButton.PointerDown += (PointerEventData data) => { isJumpPressed = true; };
				JumpButton.PointerUp += (PointerEventData data) => { isJumpPressed = false; };
			}
		}

        public override float GetHorizontalMovementInput()
		{
			if(Helper.IsMobileWebGlPlatform())
			{
				return joystick.Horizontal;
			}

			if(useRawInput)
				return Input.GetAxisRaw(horizontalInputAxis);
			else
				return Input.GetAxis(horizontalInputAxis);
		}

		public override float GetVerticalMovementInput()
		{
            if (Helper.IsMobileWebGlPlatform())
            {
                return joystick.Vertical;
            }
            if (useRawInput)
				return Input.GetAxisRaw(verticalInputAxis);
			else
				return Input.GetAxis(verticalInputAxis);
		}

		public override bool IsJumpKeyPressed()
		{
			if(Helper.IsMobileWebGlPlatform())
			{
				return isJumpPressed;
			}
			return Input.GetKey(jumpKey);
		}
    }
}
