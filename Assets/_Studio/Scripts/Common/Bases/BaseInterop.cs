using System;
using UnityEngine;
using System.Collections.Generic;

namespace Terra.Studio
{
    internal abstract class BaseInterop : IInterop
    {
        private Dictionary<Type, object> _instanceDict = new();

        public virtual void Register<T>(T instance)
        {
            _instanceDict.Add(typeof(T), instance);
        }

        public virtual void Unregister<T>(T instance)
        {
            _instanceDict.Remove(typeof(T));
        }

        public virtual T Resolve<T>()
        {
            if (!_instanceDict.ContainsKey(typeof(T)))
            {
                return default;
            }
            return (T)_instanceDict[typeof(T)];
        }
    }

    internal class Interop<T> : BaseInterop where T : IInterop
    {
        private static T _instance;

        internal static IInterop Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Activator.CreateInstance<T>();
                }
                return _instance as IInterop;
            }
        }

        internal static void Flush()
        {
            _instance = default;
        }
    }
}