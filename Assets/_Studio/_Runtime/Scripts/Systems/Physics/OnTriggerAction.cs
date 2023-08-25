using System;
using UnityEngine;

namespace Terra.Studio
{
    public class OnTriggerAction : MonoBehaviour
    {
        public Action OnTriggered { get; set; }
        public string TagAgainst { get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(TagAgainst))
            {
                return;
            }
            OnTriggered?.Invoke();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.transform.CompareTag(TagAgainst))
            {
                return;
            }
            OnTriggered?.Invoke();
        }
    }
}
