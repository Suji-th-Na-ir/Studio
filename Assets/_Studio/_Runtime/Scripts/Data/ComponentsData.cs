using System;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class ComponentsData
    {
        private Dictionary<string, Type> authorMap;
        private Dictionary<string, Type> eventMap;

        private void GetAllAuthors()
        {
            authorMap = new();
            var assembly = Assembly.GetExecutingAssembly();
            var derivedTypes = assembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(BaseAuthor)))
                .ToArray();
            foreach (var derivedType in derivedTypes)
            {
                var authorAttribute = derivedType.GetCustomAttribute<AuthorAttribute>();
                if (authorAttribute != null)
                {
                    authorMap.Add(authorAttribute.AuthorTarget, derivedType);
                }
            }
        }

        public BaseAuthor GetAuthorForType(string dataType)
        {
            if (authorMap == null)
            {
                GetAllAuthors();
            }
            if (authorMap.ContainsKey(dataType))
            {
                var type = authorMap[dataType];
                var instance = Activator.CreateInstance(type);
                return instance as BaseAuthor;
            }
            Debug.Log($"Author for type does not exist: {dataType}");
            return null;
        }

        private void GetAllEvents()
        {
            eventMap = new();
            var assembly = Assembly.GetExecutingAssembly();
            var derivedTypes = assembly.GetTypes()
                .Where(type => type.IsValueType && !type.IsEnum)
                .Where(type => type.GetInterfaces().Contains(typeof(IEventExecutor)))
                .Where(type => type.GetCustomAttribute<EventExecutorAttribute>() != null)
                .ToArray();
            foreach (var derivedType in derivedTypes)
            {
                var eventAttribute = derivedType.GetCustomAttribute<EventExecutorAttribute>();
                if (eventAttribute != null)
                {
                    eventMap.Add(eventAttribute.EventTarget, derivedType);
                }
            }
        }

        public void ProvideEventContext(string dataType, Action<object> onConditionalCheck, bool subscribe, object conditionalCheck = null)
        {
            if (eventMap == null)
            {
                GetAllEvents();
            }
            if (eventMap.ContainsKey(dataType))
            {
                var type = eventMap[dataType];
                var instance = Activator.CreateInstance(type) as IEventExecutor;
                instance.Execute(onConditionalCheck, subscribe, conditionalCheck);
            }
            else
            {
                Debug.Log($"Event for type does not exist: {dataType}");
            }
        }
    }
}
