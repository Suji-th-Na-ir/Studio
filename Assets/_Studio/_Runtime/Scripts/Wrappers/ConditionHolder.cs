using System;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class ConditionHolder
    {
        private Dictionary<string, List<Action<object>>> conditionsHolder = new();

        public IEnumerable<Action<object>> Get(string conditionName)
        {
            if (conditionsHolder.ContainsKey(conditionName))
            {
                return conditionsHolder[conditionName];
            }
            Debug.Log($"No one is listening to {conditionName}");
            return null;
        }

        public void Set(string conditionName, Action<object> action)
        {
            if (!conditionsHolder.ContainsKey(conditionName))
            {
                conditionsHolder.Add(conditionName, new List<Action<object>>());
            }
            conditionsHolder[conditionName].Add(action);
        }
    }
}
