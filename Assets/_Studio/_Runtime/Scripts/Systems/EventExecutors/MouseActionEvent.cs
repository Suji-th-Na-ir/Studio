using System;
using UnityEngine;
using System.Linq;
using UnityEngine.Scripting;
using System.Collections.Generic;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.MouseAction")]
    public struct MouseActionEvent : IEventExecutor
    {
        private List<ObjectToEvent> objectToEvents;
        private List<int> toBeRemovedEventIndices;
        private bool isListeningToEvents;

        public void Execute(bool subscribe, EventContext context)
        {
            objectToEvents ??= new();
            toBeRemovedEventIndices ??= new();
            var foundData = objectToEvents.Find(x => x.objRef == context.goRef &&
            x.componentName == context.componentName);
            if (subscribe)
            {
                if (foundData.Equals(default(ObjectToEvent)))
                {
                    var newData = new ObjectToEvent()
                    {
                        action = context.onConditionMet,
                        objRef = context.goRef,
                        componentName = context.componentName
                    };
                    objectToEvents.Add(newData);
                }
                else
                {
                    var index = objectToEvents.IndexOf(foundData);
                    var isQueuedToBeRemoved = toBeRemovedEventIndices.Any(x => x == index);
                    if (isQueuedToBeRemoved)
                    {
                        toBeRemovedEventIndices.Remove(index);
                    }
                    else
                    {
                        Debug.Log($"Already registered! For type: {context.componentName} | Go: {context.goRef}", context.goRef);
                    }
                }
            }
            else
            {
                if (!foundData.Equals(default(ObjectToEvent)))
                {
                    toBeRemovedEventIndices.Add(objectToEvents.IndexOf(foundData));
                }
                else
                {
                    Debug.Log($"Not registered! For type: {context.componentName} | Go: {context.goRef}", context.goRef);
                }
            }
            CheckForEventListen();
        }

        private readonly void OnCheck(GameObject clickedObj)
        {
            foreach (var index in toBeRemovedEventIndices)
            {
                objectToEvents.RemoveAt(index);
            }
            toBeRemovedEventIndices.Clear();
            var executables = new List<Action<object>>();
            foreach (var objEvent in objectToEvents)
            {
                if (objEvent.objRef == clickedObj)
                {
                    executables.Add(objEvent.action);
                }
            }
            foreach (var execute in executables)
            {
                execute?.Invoke(clickedObj);
            }
        }

        private void CheckForEventListen()
        {
            var mouseEventInvoker = RuntimeOp.Resolve<RuntimeSystem>() as IMouseEvents;
            if (!isListeningToEvents)
            {
                mouseEventInvoker.OnClicked += OnCheck;
                isListeningToEvents = true;
            }
            else if (objectToEvents.Count == 0)
            {
                mouseEventInvoker.OnClicked -= OnCheck;
                isListeningToEvents = false;
            }
        }

        private struct ObjectToEvent
        {
            public string componentName;
            public GameObject objRef;
            public Action<object> action;
        }
    }
}
