using System;
using UnityEngine;

namespace Terra.Studio
{
    public class OnTriggerAction : MonoBehaviour
    {
        public Action OnTriggered { get; set; }
        public string TagAgainst { get; set; }

        private void Start()
        {
            if (!TryGetComponent(out Collider collider))
            {
                collider = gameObject.AddComponent<BoxCollider>();
            }
            else
            {
                if (TryGetComponent(out MeshCollider meshCollider))
                {
                    meshCollider.convex = true;
                }
            }
            collider.isTrigger = true;
            if (!TryGetComponent(out Rigidbody rb))
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.useGravity = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(TagAgainst))
            {
                return;
            }
            OnTriggered?.Invoke();
        }
    }
}
