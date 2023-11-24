using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class ComponentsData
    {
        private static readonly string[] IGNORE_SIMULATION_TYPES = new[] { "Terra.Studio.InvokeAfter" };
        private Dictionary<string, IEventExecutor> cachedReferences = new();
        private List<EventContext> eventContexts = new();
        public bool ContinueExecutingWhileSimulationInProgress = false;

        public void ProvideEventContext(bool subscribe, EventContext context)
        {
            var isSimulating = SystemOp.Resolve<System>().IsSimulating;
            var isSimulationIgnoreType = IGNORE_SIMULATION_TYPES.Any(x => x.Equals(context.conditionType));
            if (isSimulating && !isSimulationIgnoreType)
            {
                if (subscribe)
                {
                    if (ContinueExecutingWhileSimulationInProgress)
                    {
                        context.onConditionMet?.Invoke(context.goRef);
                        return;
                    }
                    else
                    {
                        eventContexts.Add(context);
                    }
                }
                else if (eventContexts.Contains(context))
                {
                    eventContexts.Remove(context);
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
            ContinueExecutingWhileSimulationInProgress = true;
        }
    }
}
