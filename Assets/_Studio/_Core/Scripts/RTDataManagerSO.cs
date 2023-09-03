using System;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    [CreateAssetMenu(fileName = "DataManagerSO", menuName = "Terra/DataManager")]
    public class RTDataManagerSO : ScriptableObject
    {
        [Serializable]
        public class SystemData
        {
            public string Key;
            [ComponentList] public string ComponentName;
            [SystemList] public string SystemName;
            [EditorDrawerList] public string DrawerName;
        }

        [Serializable]
        public class EventData
        {
            public string Key;
            [ActionEventList]
            public string EventName;
            public StartOn EventValue;
            public string DefaultData;
        }

        private struct CachedSystemData
        {
            public string Key;
            public Type ComponentType;
            public Type SystemType;
            public Type DrawerType;
        }

        [SerializeField] private List<SystemData> systemData;
        [SerializeField] private List<EventData> eventData;
        private List<CachedSystemData> cachedSystemData = new();
        private List<Type> cachedEventTypes = new();

        public bool TryGetKeyFromDrawerType<T>(T _, out string key)
        {
            key = null;
            var type = typeof(T);
            var foundData = systemData.Find(x => x.DrawerName.Equals(type.FullName));
            if (foundData != null)
            {
                key = foundData.Key;
            }
            return key != null;
        }

        public bool TryGetComponentAndSystemForType(string type, out Type component, out Type system)
        {
            component = system = null;
            var foundData = systemData.Find(x => x.Key.Equals(type));
            if (foundData == null)
            {
                return false;
            }
            var (componentType, systemType) = GetTypesFromData(foundData);
            component = componentType;
            system = systemType;
            return true;
        }

        public bool TryGetEventForType(string type, out Type eventType)
        {
            eventType = null;
            var foundData = eventData.Find(x => x.Key.Equals(type));
            var isFound = foundData != null;
            if (isFound)
            {
                eventType = GetEventTypeFromName(foundData.EventName);
            }
            return isFound;
        }

        private (Type, Type) GetTypesFromData(SystemData typeData)
        {
            var foundData = cachedSystemData.Find(x => x.Key.Equals(typeData.Key));
            if (foundData.Equals(default(CachedSystemData)))
            {
                foundData = new CachedSystemData()
                {
                    Key = typeData.Key,
                    ComponentType = Type.GetType(typeData.ComponentName),
                    SystemType = Type.GetType(typeData.SystemName),
                    DrawerType = Type.GetType(typeData.DrawerName)
                };
                cachedSystemData.Add(foundData);
            }
            return (foundData.ComponentType, foundData.SystemType);
        }

        private Type GetEventTypeFromName(string name)
        {
            var type = cachedEventTypes.Find(x => x.FullName.Equals(name));
            if (type == null)
            {
                type = Type.GetType(name);
                cachedEventTypes.Add(type);
            }
            return type;
        }
    }
}