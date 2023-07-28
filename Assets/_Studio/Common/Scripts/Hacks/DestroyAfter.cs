using UnityEngine;
using System.Collections;

namespace Terra.Studio
{
    public class DestroyAfter : MonoBehaviour
    {
        public float seconds;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(seconds);
            Destroy(gameObject);
        }
    }
}
