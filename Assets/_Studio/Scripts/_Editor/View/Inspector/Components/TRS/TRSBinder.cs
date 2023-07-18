using System;
using UnityEngine;

namespace Terra.Studio
{
    public class TRSBinder : MonoBehaviour
    {
        public Action<Vector3> onPositionChanged;
        public Action<Vector3> onRotationChanged;
        public Action<Vector3> onScaleChanged;

        private Vector3 cachedPosition = Vector3.one;
        private Vector3 cachedRotation = Vector3.one;
        private Vector3 cachedScale = -Vector3.zero;

        private void Update()
        {
            if (transform.position != cachedPosition)
            {
                cachedPosition = transform.position;
                onPositionChanged?.Invoke(cachedPosition);
            }
            if (transform.rotation.eulerAngles != cachedRotation)
            {
                cachedRotation = transform.rotation.eulerAngles;
                onRotationChanged?.Invoke(cachedRotation);
            }
            if (transform.localScale != cachedScale)
            {
                cachedScale = transform.localScale;
                onScaleChanged?.Invoke(cachedScale);
            }
        }
    }
}
