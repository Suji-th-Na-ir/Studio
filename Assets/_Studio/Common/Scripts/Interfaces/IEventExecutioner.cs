using System;
using UnityEngine;

namespace Terra.Studio
{
    public interface IEventExecutor
    {
        public void Execute(bool subscribe, EventContext conditionalCheck);
    }

    public struct EventContext
    {
        public Action<object> onConditionMet;
        public string componentName;
        public string conditionType;
        public string conditionData;
        public GameObject goRef;
    }
}
