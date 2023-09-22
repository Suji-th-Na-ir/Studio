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
            if (cachedReferences.TryGetValue(context.conditionType, out var eventValue))
            {
                eventValue.Execute(subscribe, context);
            }
            else
            {
                var isFound = GetManagerSO().TryGetEventForType(context.conditionType, out var type);
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
