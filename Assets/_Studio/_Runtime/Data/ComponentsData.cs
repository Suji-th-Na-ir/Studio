using System;
using System.Collections;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class ComponentsData
    {
        public static BaseAuthor GetAuthorForType(string dataType)
        {
            switch (dataType)
            {
                default:
                case "Terra.Studio.OscillateComponent":
                    return new OscillateAuthor();
                case "Terra.Studio.Broadcaster":
                    return new BroadcastAuthor();
            }
        }

        public static void GetSystemForCondition(string dataType, Action<object> onConditionalCheck, bool subscribe, string conditionalCheck = null)
        {
            switch (dataType)
            {
                default:
                case "Terra.Studio.MouseAction":
                    var mouseEvents = Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>() as IMouseEvents;
                    if (subscribe)
                    {
                        mouseEvents.OnClicked += onConditionalCheck;
                    }
                    else
                    {
                        mouseEvents.OnClicked -= onConditionalCheck;
                    }
                    break;
                case "Terra.Studio.Condition":
                    if (conditionalCheck == null)
                    {
                        return;
                    }
                    if (subscribe)
                    {
                        Interop<RuntimeInterop>.Current.Resolve<ConditionHolder>().Set(conditionalCheck, onConditionalCheck);
                    }
                    break;
            }
        }
    }
}
