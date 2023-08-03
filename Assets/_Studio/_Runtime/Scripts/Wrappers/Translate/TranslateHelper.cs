using System;
using UnityEngine;
using System.Collections;

namespace Terra.Studio
{
    public class TranslateHelper : MonoBehaviour
    {
        private Vector3 translateFrom;
        private Vector3 translateTo;
        private float speed;
        private int translateTimes;
        private bool shouldPause;
        private float pauseForTime;
        private float pauseDistance;
        private bool shouldPingPong;
        private Action onTranslationDone;
        private BroadcastAt broadcastAt;

        public void Translate(TranslateParams translateParams)
        {
            translateFrom = translateParams.translateFrom;
            translateTo = translateParams.translateTo;
            speed = translateParams.speed;
            translateTimes = translateParams.translateTimes;
            shouldPause = translateParams.shouldPause;
            pauseForTime = translateParams.pauseForTime;
            shouldPingPong = translateParams.shouldPingPong;
            pauseDistance = translateParams.pauseDistance;
            broadcastAt = translateParams.broadcastAt;

            var direction = (translateTo - translateFrom).normalized;
            var distance = Vector3.Distance(translateFrom, translateTo);

            if (!shouldPingPong)
            {
                StartCoroutine(TranslateCoroutine(direction, distance));
            }
            else
            {
                StartCoroutine(OscillateCoroutine());
            }
        }

        private IEnumerator TranslateCoroutine(Vector3 direction, float distance)
        {
            var coveredDistance = 0f;
            var shouldTranslateForever = translateTimes == int.MaxValue;
            var loopsFinished = 0f;
            do
            {
                if (!shouldTranslateForever) ++loopsFinished;
                var remainingDistance = distance;
                while (remainingDistance > 0.01f)
                {
                    var step = speed * Time.deltaTime;
                    var movement = direction * step;
                    transform.position += movement;
                    remainingDistance -= step;
                    coveredDistance += step;
                    if (shouldPause && coveredDistance >= pauseDistance)
                    {
                        yield return new WaitForSeconds(pauseForTime);
                        coveredDistance = 0f;
                    }
                    yield return null;
                }
                if (broadcastAt == BroadcastAt.AtEveryInterval)
                {
                    onTranslationDone?.Invoke();
                }
            }
            while (shouldTranslateForever || loopsFinished < translateTimes);
            if (broadcastAt == BroadcastAt.End)
            {
                onTranslationDone?.Invoke();
            }
            yield return null;
            Destroy(this);
        }

        private IEnumerator OscillateCoroutine()
        {
            var coveredDistance = 0f;
            var shouldTranslateForever = translateTimes == int.MaxValue;
            var loopsFinished = 0f;
            long currentLoop = 0;
            do
            {
                if (!shouldTranslateForever) ++loopsFinished;
                var direction = currentLoop % 2 == 0 ?
                    (translateTo - translateFrom).normalized :
                    (translateFrom - translateTo).normalized;
                var comparativePos = currentLoop % 2 == 0 ?
                    translateTo :
                    translateFrom;
                var remainingDistance = Vector3.Distance(transform.position, comparativePos);
                while (remainingDistance > 0.01f)
                {
                    var step = speed * Time.deltaTime;
                    var movement = direction * step;
                    transform.position += movement;
                    remainingDistance -= step;
                    coveredDistance++;
                    if (shouldPause && coveredDistance >= pauseDistance)
                    {
                        yield return new WaitForSeconds(pauseForTime);
                        coveredDistance = 0f;
                    }
                    yield return null;
                }
                currentLoop++;
                if (broadcastAt == BroadcastAt.AtEveryInterval)
                {
                    onTranslationDone?.Invoke();
                }
            }
            while (shouldTranslateForever || loopsFinished < translateTimes * 2);
            if (broadcastAt == BroadcastAt.End)
            {
                onTranslationDone?.Invoke();
            }
            yield return null;
            Destroy(this);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
