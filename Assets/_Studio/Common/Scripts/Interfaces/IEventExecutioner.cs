using System;
using UnityEngine;

namespace Terra.Studio
{
    public interface IEventExecutor
    {
        public void Execute(Action<object> onConditionalCheck, bool subscribe, EventExecutorData conditionalCheck);
    }

    public struct EventExecutorData
    {
        public GameObject goRef;
        public string data;
    }
}
