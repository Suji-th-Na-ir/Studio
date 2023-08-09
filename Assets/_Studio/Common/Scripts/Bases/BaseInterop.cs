using System;
using System.Linq;
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
            UnstageIfDisposableType<T>();
            _instanceDict.Remove(typeof(T));
        }

        public virtual void Unregister<T>()
        {
            UnstageIfDisposableType<T>();
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

        private void UnstageIfDisposableType<T>()
        {
            var type = typeof(T);
            if (!type.GetInterfaces().Contains(typeof(IDisposable)))
            {
                return;
            }
            var disposable = (IDisposable)(T)_instanceDict[type];
            disposable.Dispose();
        }
    }

    internal class Interop<T> : BaseInterop where T : IInterop
    {
        private static T _instance;

        internal static IInterop Current
        {
            get
            {
                _instance ??= Activator.CreateInstance<T>();
                return _instance;
            }
        }

        internal static void Flush()
        {
            _instance = default;
        }
    }
}