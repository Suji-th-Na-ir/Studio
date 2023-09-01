using System;
using UnityEngine;

namespace Terra.Studio
{
    public interface IEventExecutor
    {
        public void Execute(Action<object> onConditionalCheck, bool subscribe, EventConditionalCheckData conditionalCheck);
    }

    public struct EventContext
    {
        public Action<object> onConditionMet;
        public EventConditionalCheckData data;
    }

    public struct EventConditionalCheckData
    {
        public string conditionType;
        public string conditionData;
        public GameObject goRef;
        public GameObject toCheckAgainst;
    }
}
