using UnityEngine;
using UnityEngine.Scripting;
using System.Collections.Generic;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.InvokeAfter")]
    public struct InvokeAfterActionEvent : IEventExecutor
    {
        private Dictionary<EventContext, CoroutineService> lookOutObjects;
        public bool isRunningTimer;

        public void Execute(bool subscribe, EventContext context)
        {
            Debug.Log($"Invoke execution status: {subscribe}");
            lookOutObjects ??= new();
            var isFound = lookOutObjects.TryGetValue(context, out var coroutineService);
            if (subscribe)
            {
                if (!isFound || !coroutineService)
                {
                    RegisterInvokeRepeating(context);
                }
                else
                {
                    Debug.Log($"Already registered! For type: {context.componentName} | Go: {context.goRef}", context.goRef);
                }
            }
            else
            {
                if (isFound)
                {
                    coroutineService.Stop();
                    lookOutObjects.Remove(context);
                }
            }
        }

        private readonly void RegisterInvokeRepeating(EventContext context)
        {
            var seconds = float.Parse(context.conditionData);
            var additionalData = (InvokeAfterData)context.additionalData;
            var rounds = additionalData.rounds;
            var coroutine = CoroutineService.RunCoroutineInBatches(rounds, CoroutineService.DelayType.WaitForXSeconds, seconds, null, () =>
            {
                context.onConditionMet?.Invoke(context.goRef);
            });
            lookOutObjects.Add(context, coroutine);
            if (additionalData.invokeAtStart)
            {
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                var newContext = new EventContext()
                {
                    conditionType = "Terra.Studio.GameStart",
                    conditionData = "Start",
                    componentName = context.componentName,
                    goRef = context.goRef,
                    onConditionMet = (goRef) =>
                    {
                        //Handle unsubscription sometime soon
                        context.onConditionMet?.Invoke(goRef);
                    }
                };
                compsData.ProvideEventContext(true, newContext);
            }
        }
    }
}
