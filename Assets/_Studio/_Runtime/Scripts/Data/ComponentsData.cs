using System;
using UnityEngine;

namespace Terra.Studio
{
    public class ComponentsData
    {
        public static BaseAuthor GetAuthorForType(string dataType)
        {
            switch (dataType)
            {
                default:
                    Debug.Log($"No author found for: {dataType}");
                    return null;
                case "Terra.Studio.Oscillate":
                    return new OscillateAuthor();
                case "Terra.Studio.Collectable":
                    return new CollectableAuthor();
            }
        }

        public static void GetSystemForCondition(string dataType, Action<object> onConditionalCheck, bool subscribe, object conditionalCheck = null)
        {
            switch (dataType)
            {
                default:
                    Debug.Log($"No system condition is met! {dataType}");
                    break;
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
                case "Terra.Studio.TriggerAction":
                    if (conditionalCheck == null)
                    {
                        return;
                    }
                    var tuple = ((GameObject goRef, string tagCheck))conditionalCheck;
                    var go = tuple.goRef;
                    if (subscribe)
                    {
                        var triggerAction = go.AddComponent<OnTriggerAction>();
                        triggerAction.TagAgainst = tuple.tagCheck;
                        triggerAction.onTriggered = () =>
                        {
                            triggerAction.onTriggered = null;
                            onConditionalCheck?.Invoke(null);
                        };
                    }
                    else if (go.TryGetComponent(out OnTriggerAction triggerAction1))
                    {
                        UnityEngine.Object.Destroy(triggerAction1);
                    }
                    break;
                case "Terra.Studio.Listener":
                    var broadcaster = Interop<RuntimeInterop>.Current.Resolve<Broadcaster>();
                    if (subscribe)
                    {
                        broadcaster.ListenTo((string)conditionalCheck, () => { onConditionalCheck?.Invoke(null); });
                    }
                    else
                    {
                        broadcaster.StopListenTo((string)conditionalCheck, () => { onConditionalCheck?.Invoke(null); });
                    }
                    break;
            }
        }
    }
}
