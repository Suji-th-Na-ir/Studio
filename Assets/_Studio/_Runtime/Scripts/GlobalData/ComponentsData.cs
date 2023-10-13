using System;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class ComponentsData
    {
        private Dictionary<string, IEventExecutor> cachedReferences = new();
        private List<EventContext> eventContexts = new();

        public void ProvideEventContext(bool subscribe, EventContext context)
        {
            var isSimulating = SystemOp.Resolve<System>().IsSimulating;
            if (isSimulating)
            {
                if (subscribe)
                {
                    eventContexts.Add(context);
                }
            }
            else
            {
                if (cachedReferences.TryGetValue(context.conditionType, out var eventValue))
                {
                    eventValue.Execute(subscribe, context);
                }
                else
                {
                    var isFound = SystemOp.Resolve<System>().SystemData.TryGetEventForType(context.conditionType, out var type);
                    if (isFound)
                    {
                        var instance = Activator.CreateInstance(type) as IEventExecutor;
                        instance.Execute(subscribe, context);
                        cachedReferences.Add(context.conditionType, instance);
                    }
                    else
                    {
                        Debug.LogError($"Event for type {context.conditionType} is not found!");
                    }
                }
            }
        }

        public void ExecuteAllInterceptedEvents()
        {
            for (int i = 0; i < eventContexts.Count; i++)
            {
                var context = eventContexts[i];
                context.onConditionMet?.Invoke(context.goRef);
            }
            eventContexts.Clear();
        }
    }
}
