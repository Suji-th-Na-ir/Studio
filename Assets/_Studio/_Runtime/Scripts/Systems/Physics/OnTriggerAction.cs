using System;
using UnityEngine;

namespace Terra.Studio
{
    public class OnTriggerAction : MonoBehaviour
    {
        private const string OTHER_TAG_KEY = "Other";
        private const string PLAYER_TAG_KEY = "Player";
        private const string ANY_TAG_KEY = "Any";
        public Action<GameObject> OnTriggered { get; set; }
        public string tagAgainst;

        private void OnTriggerEnter(Collider other)
        {
            CheckAndTrigger(other.transform);
        }

        private void OnCollisionEnter(Collision collision)
        {
            CheckAndTrigger(collision.transform);
        }

        private void CheckAndTrigger(Transform other)
        {
            if (tagAgainst.Equals(ANY_TAG_KEY))
            {
                OnTriggered?.Invoke(other.gameObject);
                return;
            }

            if (tagAgainst.Equals(OTHER_TAG_KEY))
            {
                if (!other.CompareTag(PLAYER_TAG_KEY))
                {
                    OnTriggered?.Invoke(other.gameObject);
                }
                return;
            }

            if (other.CompareTag(tagAgainst))
            {
                OnTriggered?.Invoke(other.gameObject);
            }
        }
    }
}
