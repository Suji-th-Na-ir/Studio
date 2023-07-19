using System;
using UnityEngine;
using System.Collections.Generic;

public class ConditionHolder
{
    private Dictionary<string, List<Action>> conditionsHolder = new();

    public IEnumerable<Action> Get(string conditionName)
    {
        if (conditionsHolder.ContainsKey(conditionName))
        {
            return conditionsHolder[conditionName];
        }
        Debug.Log($"No one is listening to {conditionName}");
        return null;
    }

    public void Set(string conditionName, Action action)
    {
        if (!conditionsHolder.ContainsKey(conditionName))
        {
            conditionsHolder.Add(conditionName, new List<Action>());
        }
        conditionsHolder[conditionName].Add(action);
    }
}
