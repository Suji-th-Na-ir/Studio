namespace Terra.Studio
{
    public class SystemOp
    {
        private static IInterop Op => Interop<SystemInterop>.Current;

        public static void Register<T>(T instance)
        {
            Op.Register(instance);
        }

        public static void Unregister<T>()
        {
            Op.Unregister<T>();
        }

        public static void Unregister<T>(T instance)
        {
            Op.Unregister(instance);
        }

        public static T Resolve<T>()
        {
            return Op.Resolve<T>();
        }

        public static void Flush()
        {
            Interop<SystemInterop>.Flush();
        }

        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            return Op.Load<T>(path);
        }

        public static UnityEngine.Object Load(ResourceTag tag, string appendPath = null)
        {
            return Op.Load(tag, appendPath);
        }

        private class SystemInterop : BaseInterop
        {
            public override T Load<T>(string path)
            {
                return ResourceLoader.Load<T>(LoadFor.System, path);
            }

            public override UnityEngine.Object Load(ResourceTag tag, string appendPath = null)
            {
                return ResourceLoader.Load(LoadFor.System, tag, appendPath);
            }
        }
    }
}
