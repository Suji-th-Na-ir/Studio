using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayShifu.Terra;
using RuntimeInspectorNamespace;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CMF
{
    //This camera input class is an example of how to get input from a connected mouse using Unity's default input system;
    //It also includes an optional mouse sensitivity setting;
    public class CameraMouseInput : CameraInput
    {
        //Mouse input axes;
        public string mouseHorizontalAxis = "Mouse X";
        public string mouseVerticalAxis = "Mouse Y";

        //Invert input options;
        public bool invertHorizontalInput = false;
        public bool invertVerticalInput = false;


        [SerializeField] PointerEventListener jumpButton;
        [SerializeField] PointerEventListener joystick;
        //Use this value to fine-tune mouse movement;
        //All mouse input will be multiplied by this value;
        public float mouseInputMultiplier = 0.01f;
        Dictionary<int,int> touches = new Dictionary<int, int>();
        Vector2 _input;
        Vector2 lastPosition;
        private void OnEnable()
        {
#if UNITY_WEBGL &&!UNITY_EDITOR
            mouseInputMultiplier *= 0.6f;
#endif
        }

        private void Awake()
        {
            jumpButton.PointerDown += (PointerEventData data) =>
            {
                if (!touches.ContainsKey(0))
                {
                    touches.Add(0, GetJustTouchFingerId(data, true));
                }
                else
                {
                    touches[0] = GetJustTouchFingerId(data, true);
                }

            };
            joystick.PointerDown += (PointerEventData data) =>
            {
                if (!touches.ContainsKey(1))
                {
                    touches.Add(1, GetJustTouchFingerId(data, true));
                }
                else
                {
                    touches[1] = GetJustTouchFingerId(data, true);
                }

            };

            jumpButton.PointerUp += (PointerEventData data) =>
            {
                if (!touches.ContainsKey(0))
                {
                    touches.Add(0, -1);
                }
                else
                {
                    touches[0] = -1;
                }

            };
            joystick.PointerUp += (PointerEventData data) =>
            {
                if (!touches.ContainsKey(1))
                {
                    touches.Add(1, -1);
                }
                else
                {
                    touches[1] = -1;
                }

            };
        }

        public override float GetHorizontalCameraInput()
        {

            if (Helper.IsMobileWebGlPlatform())
            {
                // Calculate the pixel movement based on new touches only
                bool hasPanInput = false;
                foreach (Touch touch in Input.touches)
                {
                    if (!touches.Values.Contains(touch.fingerId))
                    {
                        if (lastPosition == Vector2.zero)
                        {
                            lastPosition = touch.position;
                        }
                        _input.x = (touch.position.x - lastPosition.x);
                        lastPosition.x = touch.position.x;
                        hasPanInput = true;
                        break; // Use the first detected touch movement.
                    }
                }
                if (Input.touchCount == 0 || !hasPanInput)
                {
                    lastPosition = Vector2.zero;
                }
            }
            else
            {
                //Get raw mouse input;
                _input.x = Input.GetAxisRaw(mouseHorizontalAxis);
            }


            //Since raw mouse input is already time-based, we need to correct for this before passing the input to the camera controller;
            if (Time.timeScale > 0f && Time.deltaTime > 0f)
            {
                _input.x /= Time.deltaTime;
                _input.x *= Time.timeScale;
            }
            else
                _input.x = 0f;

            //Apply mouse sensitivity;
            _input.x *= mouseInputMultiplier;

            //Invert input;
            if (invertHorizontalInput)
                _input *= -1f;
            return _input.x;
        }

        public override float GetVerticalCameraInput()
        {
            //Get raw mouse input;
            if (Helper.IsMobileWebGlPlatform())
            {
                // Calculate the pixel movement based on new touches only.
                bool hasPanInput = false;
                foreach (Touch touch in Input.touches)
                {
                    if (!touches.Values.Contains(touch.fingerId))
                    {
                        if(lastPosition==Vector2.zero)
                        {
                            lastPosition = touch.position;
                        }
                        _input.y = -(touch.position.y - lastPosition.y);
                        lastPosition.y = touch.position.y;
                        hasPanInput = true;
                        break; // Use the first detected touch movement.
                    }
                }
                if (Input.touchCount == 0 || !hasPanInput)
                {
                    lastPosition = Vector2.zero;
                }
            }
            else
            {
                _input.y = -Input.GetAxisRaw(mouseVerticalAxis);
            }

            //Since raw mouse input is already time-based, we need to correct for this before passing the input to the camera controller;
            if (Time.timeScale > 0f && Time.deltaTime > 0f)
            {
                _input.y /= Time.deltaTime;
                _input.y *= Time.timeScale;
            }
            else
                _input.y = 0f;

            //Apply mouse sensitivity;
            _input.y *= mouseInputMultiplier;

            //Invert input;
            if (invertVerticalInput)
                _input.y *= -1f;
            return _input.y;
        }


        private int GetJustTouchFingerId(PointerEventData eventData, bool onPointerDown)
        {
            int fingerId = -1;
            if (Input.touchCount > 0 && !Application.isEditor)
            {
                var touches = Input.touches.Where(a => a.position == eventData.position).ToArray();
                if (touches.Length > 0)
                {
                    fingerId = touches.First().fingerId;
                }
                else
                {
                    int beganTouchIndex = Input.touches.ToList().FindIndex(a => a.phase == (onPointerDown ? TouchPhase.Began : TouchPhase.Ended));
                    if (beganTouchIndex == -1)
                    {
                        if (Input.touchCount == 1)
                        {
                            beganTouchIndex = 0;
                        }
                    }

                    if (beganTouchIndex >= 0)
                    {
                        fingerId = Input.touches.First().fingerId;
                    }
                }
            }
            else
            {
                fingerId = -100;
            }
            return fingerId;
        }
    }
}