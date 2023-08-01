using System;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class ComponentsData
    {
        private const string AUTHORS_RES_PATH = "Runtime/AuthorsVariants";
        private const string EVENT_RES_PATH = "Runtime/EventsVariants";

        private Dictionary<string, Type> authorMap;
        private Dictionary<string, Type> eventMap;

        private void GetAllAuthors()
        {
            authorMap = new();
            var authorRes = Resources.Load<TextAsset>(AUTHORS_RES_PATH).text;
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(authorRes);
            foreach (var item in data)
            {
                var type = Type.GetType(item.Value);
                authorMap.Add(item.Key, type);
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
            var authorRes = Resources.Load<TextAsset>(EVENT_RES_PATH).text;
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(authorRes);
            foreach (var item in data)
            {
                var type = Type.GetType(item.Value);
                eventMap.Add(item.Key, type);
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
