using System;
using UnityEngine;

namespace Terra.Studio
{
    public class ComponentsData
    {
        private RTDataManagerSO managerSO;

        public void ProvideEventContext(bool subscribe, EventContext context)
        {
            var isFound = GetManagerSO().TryGetEventForType(context.data.conditionType, out var type);
            if (isFound)
            {
                var instance = Activator.CreateInstance(type) as IEventExecutor;
                instance.Execute(context.onConditionMet, subscribe, context.data);
            }
            else
            {
                Debug.LogError($"Event for type {context.data.conditionType} is not found!");
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
