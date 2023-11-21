using System;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    [CreateAssetMenu(fileName = "DataManagerSO", menuName = "Terra/DataManager")]
    public class RTDataManagerSO : ScriptableObject
    {
        #region Serializables

        [Serializable]
        public class SystemData
        {
            public string DisplayName;
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
            public string DisplayName;
        }

        private struct CachedSystemData
        {
            public string Key;
            public Type ComponentType;
            public Type SystemType;
            public Type DrawerType;
        }

        #endregion

        public static readonly Type[] HideAddButtonForTypes = new[] { typeof(InGameTimer), typeof(GameScore) };
        public static readonly Type[] HideRemoveButtonForTypes = new[] { typeof(Checkpoint), typeof(InGameTimer), typeof(GameScore) };

        [SerializeField] private List<SystemData> systemData;
        [SerializeField] private List<EventData> eventData;
        private List<CachedSystemData> cachedSystemData = new();
        private List<Type> cachedEventTypes = new();

        public static readonly Type[] GameEssentialBehaviours = new Type[] { typeof(PlayerSpawnPoint), typeof(InGameTimer), typeof(GameScore), typeof(PlayerHealth) };

        #region Getter Methods

        public bool TryGetDefaultFromEnumOfEventType(string enumValue, out string defaultValue)
        {
            defaultValue = null;
            var foundData = eventData.Find(x => x.EventValue.ToString().Equals(enumValue));
            if (foundData != null)
            {
                defaultValue = foundData.DefaultData;
            }
            return defaultValue != null;
        }

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

        public EventData GetEventData(string conditionalKey)
        {
            var foundData = eventData.Find(x => x.Key.Equals(conditionalKey));
            return foundData;
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

        public bool TryGetEventDisplayName(string enumKey, out string displayName)
        {
            displayName = null;
            var foundData = eventData.Find(x => x.EventValue.ToString().Equals(enumKey));
            if (foundData != null)
            {
                displayName = foundData.DisplayName;
            }
            return !string.IsNullOrEmpty(displayName);
        }

        public bool TryGetSystemDisplayName(string key, out string displayName)
        {
            displayName = null;
            var foundData = systemData.Find(x => x.DrawerName.Equals(key));
            if (foundData != null)
            {
                displayName = foundData.DisplayName;
            }
            return !string.IsNullOrEmpty(displayName);
        }

        public bool CanShowComponent(Type type)
        {
            var fullName = type.FullName;
            var foundData = systemData.Find(x => x.DrawerName.Equals(fullName));
            return foundData != null;
        }

        #endregion
    }
}