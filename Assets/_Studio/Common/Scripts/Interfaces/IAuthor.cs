namespace Terra.Studio
{
    public interface IAuthor
    {
        public void Generate();
        public void Generate(object data);
        public void Degenerate(int entityID);
    }
}
