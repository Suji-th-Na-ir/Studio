using System;
using UnityEngine;

namespace Terra.Studio
{
    public class OnTriggerAction : MonoBehaviour
    {
        private const string CALLBACK_ON_ANY_KEY = "Any";
        public Action<GameObject> OnTriggered { get; set; }
        public string tagAgainst;

        private void OnTriggerEnter(Collider other)
        {
            if (!tagAgainst.Equals(CALLBACK_ON_ANY_KEY))
            {
                if (!other.CompareTag(tagAgainst))
                {
                    return;
                }
            }
            OnTriggered?.Invoke(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!tagAgainst.Equals(CALLBACK_ON_ANY_KEY))
            {
                if (!collision.transform.CompareTag(tagAgainst))
                {
                    return;
                }
            }
            OnTriggered?.Invoke(collision.transform.gameObject);
        }
    }
}
