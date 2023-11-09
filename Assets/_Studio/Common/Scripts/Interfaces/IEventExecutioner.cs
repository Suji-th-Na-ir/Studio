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
        public object additionalData;
        public GameObject goRef;
    }

    public struct ObjectToEvent
    {
        public string componentName;
        public GameObject objRef;
        public Action<object> action;
    }

    public struct InvokeAfterData
    {
        public uint rounds;
        public bool invokeAtStart;
    }
}