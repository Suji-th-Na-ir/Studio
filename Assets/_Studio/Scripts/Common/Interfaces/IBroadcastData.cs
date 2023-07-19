namespace Terra.Studio
{
    public interface IBroadcastData
    {
        public bool IsBroadcastable { get; set; }
        public string Broadcast { get; set; }
        public bool IsTargeted { get; set; }
    }
}
