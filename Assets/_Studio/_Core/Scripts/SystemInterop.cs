namespace Terra.Studio
{
    internal class SystemOp
    {
        private static IInterop Op => Interop<SystemInterop>.Current;

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
            Interop<SystemInterop>.Flush();
        }

        internal static T Load<T>(string path) where T : UnityEngine.Object
        {
            return Op.Load<T>(path);
        }

        internal static UnityEngine.Object Load(ResourceTag tag)
        {
            return Op.Load(tag);
        }

        private class SystemInterop : BaseInterop
        {
            public override T Load<T>(string path)
            {
                return ResourceLoader.Load<T>(LoadFor.System, path);
            }

            public override UnityEngine.Object Load(ResourceTag tag)
            {
                return ResourceLoader.Load(LoadFor.System, tag);
            }
        }
    }
}
