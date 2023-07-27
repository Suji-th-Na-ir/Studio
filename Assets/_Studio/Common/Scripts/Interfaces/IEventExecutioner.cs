using System;

namespace Terra.Studio
{
    public interface IEventExecutor
    {
        public void Execute(Action<object> onConditionalCheck, bool subscribe, object conditionalCheck = null);
    }
}
