namespace Terra.Studio
{
    public interface IComponent
    {
        public (string type, string data) Export();
        public void Import(EntityBasedComponent data);
        public void Import(string data);
        public string ComponentName { get; }
    }
}