using System;
using UnityEngine;
using System.Collections;
using static Terra.Studio.RuntimeWrappers;

namespace Terra.Studio
{
    public class RotateByHelper : MonoBehaviour
    {
        private Action onRotated;
        private int rotateCount = 0;
        private float rotateBy = 90f;
        private float rotationSpeed = 45f;
        private bool rotateForever = false;
        private bool canPause = false;
        private float pauseForSeconds = 0f;
        private bool shouldPingPong = false;
        private RotateComponent.Axis axis = RotateComponent.Axis.Y;
        private RotateComponent.Direction direction = RotateComponent.Direction.Clockwise;
        private RotateComponent.BroadcastAt broadcastAt = RotateComponent.BroadcastAt.Never;

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
            broadcastAt = rotateParams.broadcastAt;
            shouldPingPong = rotateParams.shouldPingPong;
            StartCoroutine(RotateCoroutine());
        }

        private IEnumerator RotateCoroutine()
        {
            var currentRotateCount = 0;
            var directionFactor = (direction == RotateComponent.Direction.Clockwise) ? 1f : -1f;
            do
            {
                if (!rotateForever) currentRotateCount++;
                var currentRotation = 0f;
                var targetRotation = currentRotation + (rotateBy * directionFactor);
                while (currentRotation * directionFactor < targetRotation * directionFactor)
                {
                    var rotationThisFrame = rotationSpeed * Time.deltaTime;
                    currentRotation += rotationThisFrame * directionFactor;
                    transform.Rotate(GetVector3(rotationThisFrame * directionFactor));
                    yield return null;
                }
                float finalRotation = targetRotation - currentRotation;
                transform.Rotate(GetVector3(finalRotation * directionFactor));
                currentRotation = targetRotation;
                if (broadcastAt == RotateComponent.BroadcastAt.AtEveryInterval)
                {
                    onRotated?.Invoke();
                }
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
            if (broadcastAt == RotateComponent.BroadcastAt.End)
            {
                onRotated?.Invoke();
            }
            Destroy(this);
        }

        private Vector3 GetVector3(float newRotation)
        {
            var eulerAngles = transform.rotation.eulerAngles;
            switch (axis)
            {
                case RotateComponent.Axis.X:
                    return new Vector3(newRotation, 0f, 0f);
                default:
                case RotateComponent.Axis.Y:
                    return new Vector3(0f, newRotation, 0f);
                case RotateComponent.Axis.Z:
                    return new Vector3(0f, 0f, newRotation);
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
