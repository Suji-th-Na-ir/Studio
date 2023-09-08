using System;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class ComponentsData
    {
        private RTDataManagerSO managerSO;
        private Dictionary<string, IEventExecutor> cachedReferences = new();

        public void ProvideEventContext(bool subscribe, EventContext context)
        {
            if (cachedReferences.TryGetValue(context.data.conditionType, out var eventValue))
            {
                eventValue.Execute(context.onConditionMet, subscribe, context.data);
            }
            else
            {
                var isFound = GetManagerSO().TryGetEventForType(context.data.conditionType, out var type);
                if (isFound)
                {
                    var instance = Activator.CreateInstance(type) as IEventExecutor;
                    instance.Execute(context.onConditionMet, subscribe, context.data);
                    cachedReferences.Add(context.data.conditionType, instance);
                }
                else
                {
                    Debug.LogError($"Event for type {context.data.conditionType} is not found!");
                }
            }
        }

        private RTDataManagerSO GetManagerSO()
        {
            if (managerSO == null)
            {
                managerSO = SystemOp.Load<RTDataManagerSO>("DataManagerSO");
            }
            return managerSO;
        }
    }
}
