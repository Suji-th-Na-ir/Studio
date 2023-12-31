namespace Terra.Studio
{
    internal class RuntimeOp
    {
        private static IInterop Op => Interop<RuntimeInterop>.Current;

        internal static void Register<T>(T instance)
        {
            Op.Register(instance);
        }

        internal static void Unregister<T>()
        {
            Op.Unregister<T>();
        }

        internal static void Unregister<T>(T instance)
        {
            Op.Unregister(instance);
        }

        internal static T Resolve<T>()
        {
            return Op.Resolve<T>();
        }

        internal static void Flush()
        {
            Interop<RuntimeInterop>.Flush();
        }

        internal static T Load<T>(string path) where T : UnityEngine.Object
        {
            return Op.Load<T>(path);
        }

        internal static UnityEngine.Object Load(ResourceTag tag, string appendPath = null)
        {
            return Op.Load(tag, appendPath);
        }

        private class RuntimeInterop : BaseInterop
        {
            public override T Load<T>(string path)
            {
                return ResourceLoader.Load<T>(LoadFor.Runtime, path);
            }

            public override UnityEngine.Object Load(ResourceTag tag, string appendPath = null)
            {
                return ResourceLoader.Load(LoadFor.Runtime, tag, appendPath);
            }
        }
    }
}
