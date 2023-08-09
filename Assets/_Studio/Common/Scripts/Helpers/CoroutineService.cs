using System;
using UnityEngine;
using System.Collections;

namespace Terra.Studio
{
    public class CoroutineService : MonoBehaviour
    {
        public enum DelayType
        {
            WaitForFrame,
            WaitForXFrames,
            WaitForXSeconds
        }

        public static void RunCoroutine(Action onCoroutineDone, DelayType delayType, int delay = 0)
        {
            var coroutineService = new GameObject("CoroutineHelper-Service");
            var coroutine = coroutineService.AddComponent<CoroutineService>();
            coroutine.delayType = delayType;
            coroutine.onPerformed = onCoroutineDone;
            coroutine.delay = delay;
            coroutine.DoCoroutine();
        }

        private DelayType delayType;
        private Action onPerformed;
        private int delay;

        public void DoCoroutine()
        {
            StartCoroutine(Perform());
        }

        private IEnumerator Perform()
        {
            switch (delayType)
            {
                case DelayType.WaitForFrame:
                    yield return null;
                    break;
                case DelayType.WaitForXFrames:
                    for (int i = 0; i < delay; i++)
                    {
                        yield return null;
                    }
                    break;
                case DelayType.WaitForXSeconds:
                    yield return new WaitForSeconds(delay);
                    break;
            }
            onPerformed?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
