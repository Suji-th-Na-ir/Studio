namespace Terra.Studio
{
    public interface IInterop
    {
        public void Register<T>(T instance);
        public void Unregister<T>(T instance);
        public T Resolve<T>();
    }
}