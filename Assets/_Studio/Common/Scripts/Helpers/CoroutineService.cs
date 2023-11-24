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
            WaitForXSeconds,
            WaitUntil,
            UntilPredicateFailed
        }

        public static CoroutineService RunCoroutine(Action onCoroutineDone, DelayType delayType, float delay = 0, Func<bool> predicate = null)
        {
            var coroutineService = new GameObject("CoroutineHelper-Service");
            DontDestroyOnLoad(coroutineService);
            var coroutine = coroutineService.AddComponent<CoroutineService>();
            coroutine.delayType = delayType;
            coroutine.onPerformed = onCoroutineDone;
            coroutine.delay = delay;
            coroutine.predicate = predicate;
            coroutine.DoSimpleCoroutine();
            return coroutine;
        }

        public static CoroutineService RunCoroutineInBatches(uint rounds, DelayType delayType, float delay, Func<bool> predicate, Action onCoroutineDone)
        {
            var coroutineService = new GameObject("BatchCoroutineHelper-Service");
            DontDestroyOnLoad(coroutineService);
            var coroutine = coroutineService.AddComponent<CoroutineService>();
            coroutine.delayType = delayType;
            coroutine.onPerformed = onCoroutineDone;
            coroutine.delay = delay;
            coroutine.predicate = predicate;
            coroutine.rounds = rounds;
            coroutine.DoBatchedCoroutine();
            return coroutine;
        }

        public static CoroutineService LerpBetweenTwoFloats(float a, float b, float rate, Action<float> onValueModified, Action onLerpDone = null)
        {
            var coroutineService = new GameObject("LerpCoroutineHelper-Service");
            DontDestroyOnLoad(coroutineService);
            var coroutine = coroutineService.AddComponent<CoroutineService>();
            coroutine.a = a;
            coroutine.b = b;
            coroutine.rate = rate;
            coroutine.onValueModified = onValueModified;
            coroutine.onPerformed = onLerpDone;
            coroutine.DoLerpCoroutine();
            return coroutine;
        }

        private Coroutine coroutine;
        private Coroutine localCoroutine;
        private Coroutine lerpCoroutine;
        private DelayType delayType;
        private Func<bool> predicate;
        private Action<float> onValueModified;
        private Action onPerformed;
        private float delay;
        private float a;
        private float b;
        private float rate;
        private uint rounds;

        public void DoSimpleCoroutine()
        {
            coroutine = StartCoroutine(PerformSimpleCoroutine());
        }

        public void DoBatchedCoroutine()
        {
            coroutine = StartCoroutine(PerformBatchedCoroutine());
        }

        public void DoLerpCoroutine()
        {
            lerpCoroutine = StartCoroutine(DoFloatLerp());
        }

        public void Stop()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            if (localCoroutine != null)
            {
                StopCoroutine(localCoroutine);
                localCoroutine = null;
            }
            if (lerpCoroutine != null)
            {
                StopCoroutine(lerpCoroutine);
                lerpCoroutine = null;
            }
            Destroy(gameObject);
        }

        private IEnumerator PerformBatchedCoroutine()
        {
            var backTrackRound = ((int)rounds) - 1;
            for (int i = 0; i < rounds; i++)
            {
                backTrackRound--;
                localCoroutine = StartCoroutine(PerformSimpleCoroutine(backTrackRound));
                yield return localCoroutine;
            }
        }

        private IEnumerator PerformSimpleCoroutine(int currentRound = -1)
        {
            switch (delayType)
            {
                case DelayType.WaitForFrame:
                    yield return null;
                    break;
                case DelayType.WaitForXFrames:
                    for (int i = 0; i < (int)delay; i++)
                    {
                        yield return null;
                    }
                    break;
                case DelayType.WaitForXSeconds:
                    yield return new WaitForSeconds(delay);
                    break;
                case DelayType.WaitUntil:
                    if (predicate != null)
                    {
                        yield return new WaitUntil(predicate);
                    }
                    break;
                case DelayType.UntilPredicateFailed:
                    while (predicate?.Invoke() ?? false)
                    {
                        onPerformed?.Invoke();
                        yield return new WaitForSeconds(delay);
                    }
                    break;
            }
            if (delayType != DelayType.UntilPredicateFailed) onPerformed?.Invoke();
            if (currentRound == -1) Destroy(gameObject);
        }

        private IEnumerator DoFloatLerp()
        {
            var delta = 0f;
            while (delta <= 1f)
            {
                delta += rate * Time.deltaTime;
                var newValue = Mathf.Lerp(a, b, delta);
                onValueModified?.Invoke(newValue);
                yield return null;
            }
            onPerformed?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            coroutine = null;
            localCoroutine = null;
            lerpCoroutine = null;
            StopAllCoroutines();
        }
    }
}
