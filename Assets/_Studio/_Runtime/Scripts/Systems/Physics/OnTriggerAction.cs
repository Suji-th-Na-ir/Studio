using System;
using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class OnTriggerAction : MonoBehaviour
    {
        public Action onTriggered { get; set; }
        public string TagAgainst { get; set; }

        private void Start()
        {
            if (!TryGetComponent(out Collider collider))
            {
                collider = gameObject.AddComponent<BoxCollider>();
            }
            collider.isTrigger = true;
            if (!TryGetComponent(out Rigidbody rb))
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(TagAgainst))
            {
                return;
            }
            onTriggered?.Invoke();
        }
    }
}
