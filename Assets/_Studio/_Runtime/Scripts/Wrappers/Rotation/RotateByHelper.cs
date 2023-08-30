using System;
using UnityEngine;
using System.Collections;

namespace Terra.Studio
{
    public class RotateByHelper : BaseCoroutineRunner
    {
        private Action<bool> onRotated;
        private int rotateCount = 0;
        private float rotateBy = 90f;
        private float rotationSpeed = 45f;
        private bool rotateForever = false;
        private bool canPause = false;
        private float pauseForSeconds = 0f;
        private bool shouldPingPong = false;
        private Axis[] axis;
        private Direction direction = Direction.Clockwise;

        public void Rotate(RotateByParams rotateParams)
        {
            rotateBy = rotateParams.rotateBy;
            rotationSpeed = rotateParams.rotationSpeed;
            rotateCount = rotateParams.rotationTimes;
            axis = rotateParams.axis;
            direction = rotateParams.direction;
            onRotated = rotateParams.onRotated;
            rotateForever = rotateCount == int.MaxValue;
            canPause = rotateParams.shouldPause;
            pauseForSeconds = rotateParams.pauseForTime;
            shouldPingPong = rotateParams.shouldPingPong;
            StartCoroutine(RotateCoroutine());
        }

        private IEnumerator RotateCoroutine()
        {
            var currentRotateCount = 0;
            var directionFactor = (direction == Direction.Clockwise) ? 1f : -1f;
            do
            {
                if (!rotateForever) currentRotateCount++;
                var currentRotation = 0f;
                var targetRotation = currentRotation + (rotateBy * directionFactor);
                while (currentRotation * directionFactor < targetRotation * directionFactor)
                {
                    var rotationThisFrame = rotationSpeed * Time.deltaTime;
                    currentRotation += rotationThisFrame * directionFactor;
                    for (int i = 0; i < axis.Length; i++)
                    {
                        transform.Rotate(GetVector3(rotationThisFrame * directionFactor, axis[i]));
                    }
                    yield return null;
                }
                float finalRotation = targetRotation - currentRotation;

                for (int i = 0; i < axis.Length; i++)
                {
                    transform.Rotate(GetVector3(finalRotation * directionFactor, axis[i]));
                }
                onRotated?.Invoke(false);
                if (canPause)
                {
                    yield return new WaitForSeconds(pauseForSeconds);
                }
                else
                {
                    yield return null;
                }
                if (shouldPingPong)
                {
                    directionFactor *= -1f;
                }
            }
            while (rotateForever || currentRotateCount < rotateCount);
            onRotated?.Invoke(true);
            Destroy(this);
        }

        private Vector3 GetVector3(float newRotation , Axis axis)
        {
            return axis switch
            {
                Axis.X => new Vector3(newRotation, 0f, 0f),
                Axis.Z => new Vector3(0f, 0f, newRotation),
                _ => new Vector3(0f, newRotation, 0f),
            };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllCoroutines();
        }
    }
}
