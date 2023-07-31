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

        private class RuntimeInterop : BaseInterop { }
    }
}
