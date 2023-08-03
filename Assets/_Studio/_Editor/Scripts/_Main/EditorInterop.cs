namespace Terra.Studio
{
    internal static class EditorOp
    {
        private static IInterop Op => Interop<EditorInterop>.Current;

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
            Interop<EditorInterop>.Flush();
        }

        private class EditorInterop : BaseInterop { }
    }
}